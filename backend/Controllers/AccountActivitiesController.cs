using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/account-activities")]
public class AccountActivitiesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AccountActivitiesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("AccountActivities.View")]
    public async Task<ActionResult<PagedResult<AccountActivityDto>>> GetAccountActivities([FromQuery] ListQueryDto query)
    {
        var activitiesQuery = _dbContext.AccountActivities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            activitiesQuery = activitiesQuery.Where(x =>
                x.Subject.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                (x.RelatedEntityType ?? string.Empty).ToLower().Contains(search));
        }

        activitiesQuery = activitiesQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = activitiesQuery.Select(x => new AccountActivityDto
        {
            Id = x.Id,
            AccountId = x.AccountId,
            ContactId = x.ContactId,
            ActivityTypeId = x.ActivityTypeId,
            Subject = x.Subject,
            Description = x.Description,
            ActivityDate = x.ActivityDate,
            DueDate = x.DueDate,
            PriorityId = x.PriorityId,
            StatusId = x.StatusId,
            OutcomeId = x.OutcomeId,
            AssignedToUserId = x.AssignedToUserId,
            RelatedEntityType = x.RelatedEntityType,
            RelatedEntityId = x.RelatedEntityId,
            IsPrivate = x.IsPrivate,
            FollowUpRequired = x.FollowUpRequired,
            FollowUpDate = x.FollowUpDate
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("AccountActivities.View")]
    public async Task<ActionResult<AccountActivityDto>> GetAccountActivity(Guid id)
    {
        var item = await _dbContext.AccountActivities
            .Where(x => x.Id == id)
            .Select(x => new AccountActivityDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                ContactId = x.ContactId,
                ActivityTypeId = x.ActivityTypeId,
                Subject = x.Subject,
                Description = x.Description,
                ActivityDate = x.ActivityDate,
                DueDate = x.DueDate,
                PriorityId = x.PriorityId,
                StatusId = x.StatusId,
                OutcomeId = x.OutcomeId,
                AssignedToUserId = x.AssignedToUserId,
                RelatedEntityType = x.RelatedEntityType,
                RelatedEntityId = x.RelatedEntityId,
                IsPrivate = x.IsPrivate,
                FollowUpRequired = x.FollowUpRequired,
                FollowUpDate = x.FollowUpDate
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("AccountActivities.Create")]
    public async Task<ActionResult<AccountActivityDto>> CreateAccountActivity(UpsertAccountActivityRequestDto dto)
    {
        var item = new AccountActivity
        {
            AccountId = dto.AccountId,
            ContactId = dto.ContactId,
            ActivityTypeId = dto.ActivityTypeId,
            Subject = dto.Subject,
            Description = dto.Description,
            ActivityDate = dto.ActivityDate,
            DueDate = dto.DueDate,
            PriorityId = dto.PriorityId,
            StatusId = dto.StatusId,
            OutcomeId = dto.OutcomeId,
            AssignedToUserId = dto.AssignedToUserId,
            RelatedEntityType = dto.RelatedEntityType,
            RelatedEntityId = dto.RelatedEntityId,
            IsPrivate = dto.IsPrivate,
            FollowUpRequired = dto.FollowUpRequired,
            FollowUpDate = dto.FollowUpDate
        };

        _dbContext.AccountActivities.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new AccountActivityDto
        {
            Id = item.Id,
            AccountId = item.AccountId,
            ContactId = item.ContactId,
            ActivityTypeId = item.ActivityTypeId,
            Subject = item.Subject,
            Description = item.Description,
            ActivityDate = item.ActivityDate,
            DueDate = item.DueDate,
            PriorityId = item.PriorityId,
            StatusId = item.StatusId,
            OutcomeId = item.OutcomeId,
            AssignedToUserId = item.AssignedToUserId,
            RelatedEntityType = item.RelatedEntityType,
            RelatedEntityId = item.RelatedEntityId,
            IsPrivate = item.IsPrivate,
            FollowUpRequired = item.FollowUpRequired,
            FollowUpDate = item.FollowUpDate
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("AccountActivities.Update")]
    public async Task<IActionResult> UpdateAccountActivity(Guid id, UpsertAccountActivityRequestDto dto)
    {
        var item = await _dbContext.AccountActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.AccountId = dto.AccountId;
        item.ContactId = dto.ContactId;
        item.ActivityTypeId = dto.ActivityTypeId;
        item.Subject = dto.Subject;
        item.Description = dto.Description;
        item.ActivityDate = dto.ActivityDate;
        item.DueDate = dto.DueDate;
        item.PriorityId = dto.PriorityId;
        item.StatusId = dto.StatusId;
        item.OutcomeId = dto.OutcomeId;
        item.AssignedToUserId = dto.AssignedToUserId;
        item.RelatedEntityType = dto.RelatedEntityType;
        item.RelatedEntityId = dto.RelatedEntityId;
        item.IsPrivate = dto.IsPrivate;
        item.FollowUpRequired = dto.FollowUpRequired;
        item.FollowUpDate = dto.FollowUpDate;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("AccountActivities.Delete")]
    public async Task<IActionResult> DeleteAccountActivity(Guid id)
    {
        var item = await _dbContext.AccountActivities.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
