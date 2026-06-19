using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class OpportunityActivitiesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OpportunityActivitiesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("api/opportunities/{opportunityId:guid}/activities")]
    [HasPermission("OpportunityActivities.View")]
    public async Task<ActionResult<PagedResult<OpportunityActivityDto>>> GetOpportunityActivities(Guid opportunityId, [FromQuery] OpportunityActivityFilterDto query)
    {
        if (!await _dbContext.Opportunities.AnyAsync(x => x.Id == opportunityId))
        {
            return NotFound();
        }

        query.OpportunityId = opportunityId;
        return await GetActivities(query);
    }

    [HttpGet("api/opportunity-activities")]
    [HasPermission("OpportunityActivities.View")]
    public async Task<ActionResult<PagedResult<OpportunityActivityDto>>> GetActivities([FromQuery] OpportunityActivityFilterDto query)
    {
        var activitiesQuery = _dbContext.OpportunityActivities.AsQueryable();

        if (query.OpportunityId.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.StatusId.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(x => x.StatusId == query.StatusId.Value);
        }

        if (query.ActivityTypeId.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(x => x.ActivityTypeId == query.ActivityTypeId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(x => x.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            activitiesQuery = activitiesQuery.Where(x =>
                x.Subject.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.Opportunity.Topic.ToLower().Contains(search) ||
                x.ActivityType.Name.ToLower().Contains(search));
        }

        activitiesQuery = activitiesQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            activitiesQuery = activitiesQuery.OrderByDescending(x => x.ActivityDate);
        }

        return Ok(await ProjectActivities(activitiesQuery).ToPagedAsync(query));
    }

    [HttpGet("api/opportunity-activities/{id:guid}")]
    [HasPermission("OpportunityActivities.View")]
    public async Task<ActionResult<OpportunityActivityDto>> GetActivity(Guid id)
    {
        var activity = await ProjectActivities(_dbContext.OpportunityActivities.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return activity is null ? NotFound() : Ok(activity);
    }

    [HttpPost("api/opportunities/{opportunityId:guid}/activities")]
    [HasPermission("OpportunityActivities.Create")]
    public async Task<ActionResult<OpportunityActivityDto>> CreateActivity(Guid opportunityId, UpsertOpportunityActivityRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == opportunityId);
        if (opportunity is null)
        {
            return NotFound();
        }

        var statusId = await ResolveActivityStatusAsync(dto.StatusId, "OPEN");
        if (!statusId.HasValue)
        {
            return BadRequest("Activity status is required.");
        }

        dto.StatusId = statusId.Value;

        var validationError = await ValidateActivityReferencesAsync(opportunity, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var activity = new OpportunityActivity { OpportunityId = opportunityId };
        ApplyActivityValues(activity, dto);

        _dbContext.OpportunityActivities.Add(activity);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectActivities(_dbContext.OpportunityActivities.Where(x => x.Id == activity.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Opportunity activity was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("api/opportunity-activities/{id:guid}")]
    [HasPermission("OpportunityActivities.Update")]
    public async Task<IActionResult> UpdateActivity(Guid id, UpsertOpportunityActivityRequestDto dto)
    {
        var activity = await _dbContext.OpportunityActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == activity.OpportunityId);
        if (opportunity is null)
        {
            return BadRequest("Opportunity is invalid.");
        }

        var statusId = await ResolveActivityStatusAsync(dto.StatusId, "OPEN");
        if (!statusId.HasValue)
        {
            return BadRequest("Activity status is required.");
        }

        dto.StatusId = statusId.Value;

        var validationError = await ValidateActivityReferencesAsync(opportunity, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyActivityValues(activity, dto);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("api/opportunity-activities/{id:guid}/complete")]
    [HasPermission("OpportunityActivities.Complete")]
    public async Task<IActionResult> CompleteActivity(Guid id, CompleteOpportunityActivityRequestDto dto)
    {
        var activity = await _dbContext.OpportunityActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var statusId = dto.StatusId ?? await ResolveActivityStatusAsync(Guid.Empty, "COMPLETED");
        if (!statusId.HasValue)
        {
            return BadRequest("Completed activity status is not configured.");
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == statusId.Value && x.LookupCategory.Code == "ACTIVITY_STATUS"))
        {
            return BadRequest("Activity status is invalid.");
        }

        activity.CompletedDate = dto.CompletedDate ?? DateTime.UtcNow;
        activity.StatusId = statusId.Value;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/opportunity-activities/{id:guid}")]
    [HasPermission("OpportunityActivities.Delete")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        var activity = await _dbContext.OpportunityActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        activity.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateActivityReferencesAsync(Opportunity opportunity, UpsertOpportunityActivityRequestDto dto)
    {
        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ActivityTypeId && x.LookupCategory.Code == "ACTIVITY_TYPE"))
        {
            return "Activity type is required.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.StatusId && x.LookupCategory.Code == "ACTIVITY_STATUS"))
        {
            return "Activity status is required.";
        }

        if (dto.PriorityId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.PriorityId.Value && x.LookupCategory.Code == "PRIORITY"))
        {
            return "Priority is invalid.";
        }

        if (dto.ContactId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value && x.AccountId == opportunity.AccountId))
        {
            return "Contact must belong to the opportunity account.";
        }

        if (dto.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value && !x.IsDeleted))
        {
            return "Assigned user is invalid.";
        }

        if (string.IsNullOrWhiteSpace(dto.Subject))
        {
            return "Subject is required.";
        }

        return null;
    }

    private async Task<Guid?> ResolveActivityStatusAsync(Guid requestedId, string defaultCode)
    {
        if (requestedId != Guid.Empty)
        {
            return requestedId;
        }

        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "ACTIVITY_STATUS" && x.Code == defaultCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static void ApplyActivityValues(OpportunityActivity activity, UpsertOpportunityActivityRequestDto dto)
    {
        activity.ContactId = dto.ContactId;
        activity.ActivityTypeId = dto.ActivityTypeId;
        activity.Subject = dto.Subject.Trim();
        activity.Description = TrimToNull(dto.Description);
        activity.ActivityDate = dto.ActivityDate == default ? DateTime.UtcNow : dto.ActivityDate;
        activity.DueDate = dto.DueDate;
        activity.CompletedDate = dto.CompletedDate;
        activity.StatusId = dto.StatusId;
        activity.PriorityId = dto.PriorityId;
        activity.AssignedToUserId = dto.AssignedToUserId;
    }

    private static IQueryable<OpportunityActivityDto> ProjectActivities(IQueryable<OpportunityActivity> query)
    {
        return query.Select(x => new OpportunityActivityDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            ActivityTypeId = x.ActivityTypeId,
            ActivityTypeName = x.ActivityType.Name,
            Subject = x.Subject,
            Description = x.Description,
            ActivityDate = x.ActivityDate,
            DueDate = x.DueDate,
            CompletedDate = x.CompletedDate,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            PriorityId = x.PriorityId,
            PriorityName = x.Priority != null ? x.Priority.Name : null,
            AssignedToUserId = x.AssignedToUserId,
            AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.Email : null
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
