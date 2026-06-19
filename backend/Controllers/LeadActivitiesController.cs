using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class LeadActivitiesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILeadScoringService _leadScoringService;

    public LeadActivitiesController(AppDbContext dbContext, ILeadScoringService leadScoringService)
    {
        _dbContext = dbContext;
        _leadScoringService = leadScoringService;
    }

    [HttpGet("api/leads/{leadId:guid}/activities")]
    [HasPermission("LeadActivities.View")]
    public async Task<ActionResult<PagedResult<LeadActivityDto>>> GetLeadActivities(Guid leadId, [FromQuery] LeadActivityFilterDto query)
    {
        if (!await _dbContext.Leads.AnyAsync(x => x.Id == leadId))
        {
            return NotFound();
        }

        query.LeadId = leadId;
        return await GetActivities(query);
    }

    [HttpGet("api/lead-activities")]
    [HasPermission("LeadActivities.View")]
    public async Task<ActionResult<PagedResult<LeadActivityDto>>> GetActivities([FromQuery] LeadActivityFilterDto query)
    {
        var activitiesQuery = _dbContext.LeadActivities.AsQueryable();

        if (query.LeadId.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(x => x.LeadId == query.LeadId.Value);
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
                x.Lead.Topic.ToLower().Contains(search) ||
                x.ActivityType.Name.ToLower().Contains(search));
        }

        activitiesQuery = activitiesQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            activitiesQuery = activitiesQuery.OrderByDescending(x => x.ActivityDate);
        }

        return Ok(await ProjectActivities(activitiesQuery).ToPagedAsync(query));
    }

    [HttpGet("api/lead-activities/{id:guid}")]
    [HasPermission("LeadActivities.View")]
    public async Task<ActionResult<LeadActivityDto>> GetActivity(Guid id)
    {
        var item = await ProjectActivities(_dbContext.LeadActivities.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("api/leads/{leadId:guid}/activities")]
    [HasPermission("LeadActivities.Create")]
    public async Task<ActionResult<LeadActivityDto>> CreateActivity(Guid leadId, UpsertLeadActivityRequestDto dto)
    {
        if (dto.LeadId != leadId)
        {
            return BadRequest("Activity lead must match the route lead.");
        }

        var validationError = await ValidateActivityReferencesAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var activity = new LeadActivity();
        ApplyActivityValues(activity, dto);
        _dbContext.LeadActivities.Add(activity);
        await _dbContext.SaveChangesAsync();

        if (activity.CompletedDate.HasValue)
        {
            await _leadScoringService.RecalculateLeadScoreAsync(activity.LeadId);
        }

        var created = await ProjectActivities(_dbContext.LeadActivities.Where(x => x.Id == activity.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Lead activity was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("api/lead-activities/{id:guid}")]
    [HasPermission("LeadActivities.Update")]
    public async Task<IActionResult> UpdateActivity(Guid id, UpsertLeadActivityRequestDto dto)
    {
        var activity = await _dbContext.LeadActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var wasIncomplete = !activity.CompletedDate.HasValue;
        var validationError = await ValidateActivityReferencesAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyActivityValues(activity, dto);
        await _dbContext.SaveChangesAsync();

        if (wasIncomplete && activity.CompletedDate.HasValue)
        {
            await _leadScoringService.RecalculateLeadScoreAsync(activity.LeadId);
        }

        return NoContent();
    }

    [HttpDelete("api/lead-activities/{id:guid}")]
    [HasPermission("LeadActivities.Delete")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        var activity = await _dbContext.LeadActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var leadId = activity.LeadId;
        activity.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await _leadScoringService.RecalculateLeadScoreAsync(leadId);
        return NoContent();
    }

    [HttpPost("api/lead-activities/{id:guid}/complete")]
    [HasPermission("LeadActivities.Complete")]
    public async Task<IActionResult> CompleteActivity(Guid id, CompleteLeadActivityRequestDto dto)
    {
        var activity = await _dbContext.LeadActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var statusId = dto.StatusId ?? await GetLookupValueIdAsync("ACTIVITY_STATUS", "COMPLETED");
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
        await _leadScoringService.RecalculateLeadScoreAsync(activity.LeadId);
        return NoContent();
    }

    private async Task<string?> ValidateActivityReferencesAsync(UpsertLeadActivityRequestDto dto)
    {
        if (!await _dbContext.Leads.AnyAsync(x => x.Id == dto.LeadId))
        {
            return "Lead is required.";
        }

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

        if (dto.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value && !x.IsDeleted))
        {
            return "Assigned user is invalid.";
        }

        return null;
    }

    private async Task<Guid?> GetLookupValueIdAsync(string categoryCode, string valueCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.Code == valueCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static void ApplyActivityValues(LeadActivity activity, UpsertLeadActivityRequestDto dto)
    {
        activity.LeadId = dto.LeadId;
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

    private static IQueryable<LeadActivityDto> ProjectActivities(IQueryable<LeadActivity> query)
    {
        return query.Select(x => new LeadActivityDto
        {
            Id = x.Id,
            LeadId = x.LeadId,
            LeadTopic = x.Lead.Topic,
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
