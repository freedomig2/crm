using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivitiesController : ControllerBase
{
    private const string ActivityTypeCategoryCode = "ACTIVITY_TYPE";
    private const string ActivityStatusCategoryCode = "ACTIVITY_STATUS";
    private const string PriorityCategoryCode = "PRIORITY";
    private const string ActivityOutcomeCategoryCode = "ACTIVITY_OUTCOME";

    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public ActivitiesController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("Activities.View")]
    public async Task<ActionResult<PagedResult<ActivityDto>>> GetActivities([FromQuery] ActivityFilterDto query)
    {
        var activities = _dbContext.Activities.AsQueryable();

        if (query.ActivityTypeId.HasValue)
        {
            activities = activities.Where(x => x.ActivityTypeId == query.ActivityTypeId.Value);
        }

        if (query.StatusId.HasValue)
        {
            activities = activities.Where(x => x.StatusId == query.StatusId.Value);
        }

        if (query.PriorityId.HasValue)
        {
            activities = activities.Where(x => x.PriorityId == query.PriorityId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            activities = activities.Where(x => x.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.AccountId.HasValue)
        {
            activities = activities.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.ContactId.HasValue)
        {
            activities = activities.Where(x => x.ContactId == query.ContactId.Value);
        }

        if (query.LeadId.HasValue)
        {
            activities = activities.Where(x => x.LeadId == query.LeadId.Value);
        }

        if (query.OpportunityId.HasValue)
        {
            activities = activities.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.CaseId.HasValue)
        {
            activities = activities.Where(x => x.CaseId == query.CaseId.Value);
        }

        if (query.IsPrivate.HasValue)
        {
            activities = activities.Where(x => x.IsPrivate == query.IsPrivate.Value);
        }

        if (query.IsActive.HasValue)
        {
            activities = activities.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.ActivityDateFrom.HasValue)
        {
            activities = activities.Where(x => x.ActivityDate >= query.ActivityDateFrom.Value);
        }

        if (query.ActivityDateTo.HasValue)
        {
            activities = activities.Where(x => x.ActivityDate <= query.ActivityDateTo.Value);
        }

        if (query.DueDateFrom.HasValue)
        {
            activities = activities.Where(x => x.DueDate.HasValue && x.DueDate >= query.DueDateFrom.Value);
        }

        if (query.DueDateTo.HasValue)
        {
            activities = activities.Where(x => x.DueDate.HasValue && x.DueDate <= query.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            activities = activities.Where(x =>
                x.ActivityNumber.ToLower().Contains(search) ||
                x.Subject.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.ActivityType.Name.ToLower().Contains(search) ||
                x.Status.Name.ToLower().Contains(search) ||
                (x.Account != null && x.Account.Name.ToLower().Contains(search)) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Lead != null && x.Lead.Topic.ToLower().Contains(search)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(search)) ||
                (x.Case != null && x.Case.CaseNumber.ToLower().Contains(search)));
        }

        activities = activities.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            activities = activities.OrderByDescending(x => x.ActivityDate).ThenByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectActivities(activities).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Activities.View")]
    public async Task<ActionResult<ActivityDto>> GetActivity(Guid id)
    {
        var item = await ProjectActivities(_dbContext.Activities.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Activities.Create")]
    public async Task<ActionResult<ActivityDto>> CreateActivity(UpsertActivityRequestDto dto)
    {
        var validationError = await ValidateActivityAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var activity = new Activity();
        ApplyActivityValues(activity, dto, isCreate: true);

        if (string.IsNullOrWhiteSpace(activity.ActivityNumber))
        {
            activity.ActivityNumber = await _numberSequenceService.GenerateNextAsync("ACTIVITY");
        }

        if (await _dbContext.Activities.AnyAsync(x => x.ActivityNumber == activity.ActivityNumber))
        {
            return BadRequest("Activity number already exists.");
        }

        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectActivities(_dbContext.Activities.Where(x => x.Id == activity.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Activity was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Activities.Update")]
    public async Task<IActionResult> UpdateActivity(Guid id, UpsertActivityRequestDto dto)
    {
        var activity = await _dbContext.Activities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var validationError = await ValidateActivityAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyActivityValues(activity, dto, isCreate: false);

        if (string.IsNullOrWhiteSpace(activity.ActivityNumber))
        {
            activity.ActivityNumber = await _numberSequenceService.GenerateNextAsync("ACTIVITY");
        }

        if (await _dbContext.Activities.AnyAsync(x => x.Id != id && x.ActivityNumber == activity.ActivityNumber))
        {
            return BadRequest("Activity number already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Activities.Delete")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        var activity = await _dbContext.Activities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        activity.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Activities.View")]
    public async Task<ActionResult<ActivityLookupDto>> GetLookup()
    {
        return Ok(new ActivityLookupDto
        {
            ActivityTypes = await GetLookupOptionsAsync(ActivityTypeCategoryCode),
            ActivityStatuses = await GetLookupOptionsAsync(ActivityStatusCategoryCode),
            Priorities = await GetLookupOptionsAsync(PriorityCategoryCode),
            Outcomes = await GetLookupOptionsAsync(ActivityOutcomeCategoryCode),
        });
    }

    [HttpGet("{activityId:guid}/comments")]
    [HasPermission("ActivityComments.View")]
    public async Task<ActionResult<PagedResult<ActivityCommentDto>>> GetComments(Guid activityId, [FromQuery] ActivityCommentFilterDto query)
    {
        if (!await _dbContext.Activities.AnyAsync(x => x.Id == activityId))
        {
            return NotFound();
        }

        query.ActivityId = activityId;
        var comments = _dbContext.ActivityComments.AsQueryable();

        if (query.ActivityId.HasValue)
        {
            comments = comments.Where(x => x.ActivityId == query.ActivityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            comments = comments.Where(x => x.CommentText.ToLower().Contains(search));
        }

        comments = comments.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            comments = comments.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectComments(comments).ToPagedAsync(query));
    }

    [HttpPost("{activityId:guid}/comments")]
    [HasPermission("ActivityComments.Create")]
    public async Task<ActionResult<ActivityCommentDto>> AddComment(Guid activityId, AddActivityCommentRequestDto dto)
    {
        if (!await _dbContext.Activities.AnyAsync(x => x.Id == activityId))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.CommentText))
        {
            return BadRequest("Comment text is required.");
        }

        var comment = new ActivityComment
        {
            ActivityId = activityId,
            CommentText = dto.CommentText.Trim(),
            IsInternal = dto.IsInternal,
        };

        _dbContext.ActivityComments.Add(comment);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectComments(_dbContext.ActivityComments.Where(x => x.Id == comment.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Comment was created but could not be loaded.") : Ok(created);
    }

    [HttpDelete("comments/{id:guid}")]
    [HasPermission("ActivityComments.Delete")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await _dbContext.ActivityComments.FirstOrDefaultAsync(x => x.Id == id);
        if (comment is null)
        {
            return NotFound();
        }

        comment.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    [HasPermission("Activities.Complete")]
    public async Task<IActionResult> CompleteActivity(Guid id, CompleteActivityRequestDto dto)
    {
        var activity = await _dbContext.Activities.FirstOrDefaultAsync(x => x.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var statusId = dto.StatusId ?? await GetLookupValueIdAsync(ActivityStatusCategoryCode, "COMPLETED");
        if (!statusId.HasValue)
        {
            return BadRequest("Completed activity status is not configured.");
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == statusId.Value && x.LookupCategory.Code == ActivityStatusCategoryCode))
        {
            return BadRequest("Activity status is invalid.");
        }

        activity.CompletedDate = dto.CompletedDate ?? DateTime.UtcNow;
        activity.StatusId = statusId.Value;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateActivityAsync(Guid? activityId, UpsertActivityRequestDto dto)
    {
        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ActivityTypeId && x.LookupCategory.Code == ActivityTypeCategoryCode))
        {
            return "Activity type is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.StatusId && x.LookupCategory.Code == ActivityStatusCategoryCode))
        {
            return "Activity status is invalid.";
        }

        if (dto.PriorityId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.PriorityId.Value && x.LookupCategory.Code == PriorityCategoryCode))
        {
            return "Priority is invalid.";
        }

        if (dto.OutcomeId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.OutcomeId.Value && x.LookupCategory.Code == ActivityOutcomeCategoryCode))
        {
            return "Outcome is invalid.";
        }

        if (dto.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value && !x.IsDeleted))
        {
            return "Assigned user is invalid.";
        }

        if (dto.AccountId.HasValue && !await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId.Value))
        {
            return "Account is invalid.";
        }

        if (dto.ContactId.HasValue)
        {
            if (!await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value))
            {
                return "Contact is invalid.";
            }

            if (dto.AccountId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value && x.AccountId == dto.AccountId.Value))
            {
                return "Contact must belong to selected account.";
            }
        }

        if (dto.LeadId.HasValue && !await _dbContext.Leads.AnyAsync(x => x.Id == dto.LeadId.Value))
        {
            return "Lead is invalid.";
        }

        if (dto.OpportunityId.HasValue && !await _dbContext.Opportunities.AnyAsync(x => x.Id == dto.OpportunityId.Value))
        {
            return "Opportunity is invalid.";
        }

        if (dto.CaseId.HasValue && !await _dbContext.ServiceCases.AnyAsync(x => x.Id == dto.CaseId.Value))
        {
            return "Case is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value && !x.IsDeleted))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (string.IsNullOrWhiteSpace(dto.Subject))
        {
            return "Subject is required.";
        }

        var activityDate = dto.ActivityDate ?? DateTime.UtcNow;
        if (dto.DueDate.HasValue && dto.DueDate.Value < activityDate)
        {
            return "Due date must be greater than or equal to activity date.";
        }

        if (dto.CompletedDate.HasValue && dto.CompletedDate.Value < activityDate)
        {
            return "Completed date must be greater than or equal to activity date.";
        }

        if (dto.ReminderAt.HasValue && dto.DueDate.HasValue && dto.ReminderAt.Value > dto.DueDate.Value)
        {
            return "Reminder time must be less than or equal to due date.";
        }

        if (!string.IsNullOrWhiteSpace(dto.ActivityNumber))
        {
            var number = dto.ActivityNumber.Trim();
            var exists = await _dbContext.Activities.AnyAsync(x => x.Id != activityId && x.ActivityNumber == number);
            if (exists)
            {
                return "Activity number already exists.";
            }
        }

        return null;
    }

    private void ApplyActivityValues(Activity activity, UpsertActivityRequestDto dto, bool isCreate)
    {
        activity.ActivityNumber = TrimToNull(dto.ActivityNumber) ?? string.Empty;
        activity.ActivityTypeId = dto.ActivityTypeId;
        activity.StatusId = dto.StatusId;
        activity.PriorityId = dto.PriorityId;
        activity.Subject = dto.Subject.Trim();
        activity.Description = TrimToNull(dto.Description);
        activity.ActivityDate = dto.ActivityDate ?? (isCreate ? DateTime.UtcNow : activity.ActivityDate);
        activity.DueDate = dto.DueDate;
        activity.CompletedDate = dto.CompletedDate;
        activity.AssignedToUserId = dto.AssignedToUserId;
        activity.AccountId = dto.AccountId;
        activity.ContactId = dto.ContactId;
        activity.LeadId = dto.LeadId;
        activity.OpportunityId = dto.OpportunityId;
        activity.CaseId = dto.CaseId;
        activity.IsPrivate = dto.IsPrivate;
        activity.OutcomeId = dto.OutcomeId;
        activity.ReminderAt = dto.ReminderAt;
        activity.IsActive = dto.IsActive;
        activity.OwnerUserId = dto.OwnerUserId;
        activity.OwnerTeamId = dto.OwnerTeamId;
    }

    private async Task<Guid?> GetLookupValueIdAsync(string categoryCode, string valueCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.Code == valueCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<List<LookupOptionDto>> GetLookupOptionsAsync(string categoryCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
            })
            .ToListAsync();
    }

    private static IQueryable<ActivityDto> ProjectActivities(IQueryable<Activity> query)
    {
        return query.Select(x => new ActivityDto
        {
            Id = x.Id,
            ActivityNumber = x.ActivityNumber,
            ActivityTypeId = x.ActivityTypeId,
            ActivityTypeName = x.ActivityType.Name,
            ActivityTypeCode = x.ActivityType.Code,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StatusCode = x.Status.Code,
            PriorityId = x.PriorityId,
            PriorityName = x.Priority != null ? x.Priority.Name : null,
            Subject = x.Subject,
            Description = x.Description,
            ActivityDate = x.ActivityDate,
            DueDate = x.DueDate,
            CompletedDate = x.CompletedDate,
            AssignedToUserId = x.AssignedToUserId,
            AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName : null,
            AccountId = x.AccountId,
            AccountName = x.Account != null ? x.Account.Name : null,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            LeadId = x.LeadId,
            LeadTopic = x.Lead != null ? x.Lead.Topic : null,
            OpportunityId = x.OpportunityId,
            OpportunityTopic = x.Opportunity != null ? x.Opportunity.Topic : null,
            CaseId = x.CaseId,
            CaseNumber = x.Case != null ? x.Case.CaseNumber : null,
            IsPrivate = x.IsPrivate,
            OutcomeId = x.OutcomeId,
            OutcomeName = x.Outcome != null ? x.Outcome.Name : null,
            ReminderAt = x.ReminderAt,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static IQueryable<ActivityCommentDto> ProjectComments(IQueryable<ActivityComment> query)
    {
        return query.Select(x => new ActivityCommentDto
        {
            Id = x.Id,
            ActivityId = x.ActivityId,
            CommentText = x.CommentText,
            IsInternal = x.IsInternal,
            CreatedAt = x.CreatedAt,
            CreatedById = x.CreatedById,
            CreatedByName = x.CreatedById.HasValue
                ? x.Activity.AssignedToUser != null && x.Activity.AssignedToUser.Id == x.CreatedById.Value
                    ? x.Activity.AssignedToUser.FirstName + " " + x.Activity.AssignedToUser.LastName
                    : x.Activity.OwnerUser != null && x.Activity.OwnerUser.Id == x.CreatedById.Value
                        ? x.Activity.OwnerUser.FirstName + " " + x.Activity.OwnerUser.LastName
                        : null
                : null,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
