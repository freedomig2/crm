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
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private const string NotificationChannelCategoryCode = "NOTIFICATION_CHANNEL";
    private const string NotificationStatusCategoryCode = "NOTIFICATION_STATUS";
    private const string PriorityCategoryCode = "PRIORITY";

    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public NotificationsController(AppDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [HasPermission("Notifications.View")]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications([FromQuery] NotificationFilterDto query)
    {
        var notifications = _dbContext.Notifications.AsQueryable();

        if (query.RecipientUserId.HasValue)
        {
            notifications = notifications.Where(x => x.RecipientUserId == query.RecipientUserId.Value);
        }

        if (query.StatusId.HasValue)
        {
            notifications = notifications.Where(x => x.StatusId == query.StatusId.Value);
        }

        if (query.ChannelId.HasValue)
        {
            notifications = notifications.Where(x => x.ChannelId == query.ChannelId.Value);
        }

        if (query.PriorityId.HasValue)
        {
            notifications = notifications.Where(x => x.PriorityId == query.PriorityId.Value);
        }

        if (query.IsDismissed.HasValue)
        {
            notifications = notifications.Where(x => x.IsDismissed == query.IsDismissed.Value);
        }

        if (query.IsActive.HasValue)
        {
            notifications = notifications.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            notifications = notifications.Where(x =>
                x.Subject.ToLower().Contains(search) ||
                x.Message.ToLower().Contains(search) ||
                x.Status.Name.ToLower().Contains(search) ||
                (x.RecipientUser.Email ?? string.Empty).ToLower().Contains(search));
        }

        notifications = notifications.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            notifications = notifications.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectNotifications(notifications).ToPagedAsync(query));
    }

    [HttpGet("mine")]
    [HasPermission("Notifications.View")]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetMyNotifications([FromQuery] NotificationFilterDto query)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        query.RecipientUserId = userId.Value;
        return await GetNotifications(query);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Notifications.View")]
    public async Task<ActionResult<NotificationDto>> GetNotification(Guid id)
    {
        var item = await ProjectNotifications(_dbContext.Notifications.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Notifications.Create")]
    public async Task<ActionResult<NotificationDto>> CreateNotification(UpsertNotificationRequestDto dto)
    {
        var validationError = await ValidateNotificationAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var notification = new Notification();
        ApplyNotificationValues(notification, dto);
        notification.SentAt ??= DateTime.UtcNow;

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectNotifications(_dbContext.Notifications.Where(x => x.Id == notification.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Notification was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Notifications.Update")]
    public async Task<IActionResult> UpdateNotification(Guid id, UpsertNotificationRequestDto dto)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (notification is null)
        {
            return NotFound();
        }

        var validationError = await ValidateNotificationAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyNotificationValues(notification, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Notifications.Delete")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (notification is null)
        {
            return NotFound();
        }

        notification.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-read")]
    [HasPermission("Notifications.MarkRead")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = _currentUserContext.UserId;
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (notification is null)
        {
            return NotFound();
        }

        if (userId.HasValue && notification.RecipientUserId != userId.Value && !User.HasClaim("permission", "Notifications.Update"))
        {
            return Forbid();
        }

        var readStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == NotificationStatusCategoryCode && x.Code == "READ")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!readStatusId.HasValue)
        {
            return BadRequest("READ notification status is not configured.");
        }

        notification.StatusId = readStatusId.Value;
        notification.ReadAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("mine/mark-all-read")]
    [HasPermission("Notifications.MarkRead")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var readStatusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == NotificationStatusCategoryCode && x.Code == "READ")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!readStatusId.HasValue)
        {
            return BadRequest("READ notification status is not configured.");
        }

        var unread = await _dbContext.Notifications
            .Where(x => x.RecipientUserId == userId.Value && x.Status.Code != "READ")
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var item in unread)
        {
            item.StatusId = readStatusId.Value;
            item.ReadAt = now;
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Notifications.View")]
    public async Task<ActionResult<NotificationLookupDto>> GetLookup()
    {
        return Ok(new NotificationLookupDto
        {
            Channels = await GetLookupOptionsAsync(NotificationChannelCategoryCode),
            Statuses = await GetLookupOptionsAsync(NotificationStatusCategoryCode),
            Priorities = await GetLookupOptionsAsync(PriorityCategoryCode),
        });
    }

    private async Task<string?> ValidateNotificationAsync(UpsertNotificationRequestDto dto)
    {
        if (!await _dbContext.Users.AnyAsync(x => x.Id == dto.RecipientUserId && !x.IsDeleted))
        {
            return "Recipient user is invalid.";
        }

        if (dto.NotificationTemplateId.HasValue && !await _dbContext.NotificationTemplates.AnyAsync(x => x.Id == dto.NotificationTemplateId.Value))
        {
            return "Notification template is invalid.";
        }

        if (dto.ChannelId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ChannelId.Value && x.LookupCategory.Code == NotificationChannelCategoryCode))
        {
            return "Notification channel is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.StatusId && x.LookupCategory.Code == NotificationStatusCategoryCode))
        {
            return "Notification status is invalid.";
        }

        if (dto.PriorityId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.PriorityId.Value && x.LookupCategory.Code == PriorityCategoryCode))
        {
            return "Notification priority is invalid.";
        }

        if (string.IsNullOrWhiteSpace(dto.Subject))
        {
            return "Subject is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return "Message is required.";
        }

        return null;
    }

    private static void ApplyNotificationValues(Notification notification, UpsertNotificationRequestDto dto)
    {
        notification.RecipientUserId = dto.RecipientUserId;
        notification.NotificationTemplateId = dto.NotificationTemplateId;
        notification.ChannelId = dto.ChannelId;
        notification.StatusId = dto.StatusId;
        notification.PriorityId = dto.PriorityId;
        notification.Subject = dto.Subject.Trim();
        notification.Message = dto.Message.Trim();
        notification.ActionUrl = string.IsNullOrWhiteSpace(dto.ActionUrl) ? null : dto.ActionUrl.Trim();
        notification.RelatedEntityType = string.IsNullOrWhiteSpace(dto.RelatedEntityType) ? null : dto.RelatedEntityType.Trim();
        notification.RelatedEntityId = dto.RelatedEntityId;
        notification.SentAt = dto.SentAt;
        notification.ReadAt = dto.ReadAt;
        notification.IsDismissed = dto.IsDismissed;
        notification.IsActive = dto.IsActive;
    }

    private static IQueryable<NotificationDto> ProjectNotifications(IQueryable<Notification> query)
    {
        return query.Select(x => new NotificationDto
        {
            Id = x.Id,
            RecipientUserId = x.RecipientUserId,
            RecipientUserEmail = x.RecipientUser.Email,
            NotificationTemplateId = x.NotificationTemplateId,
            NotificationTemplateName = x.NotificationTemplate != null ? x.NotificationTemplate.Name : null,
            ChannelId = x.ChannelId,
            ChannelName = x.Channel != null ? x.Channel.Name : null,
            ChannelCode = x.Channel != null ? x.Channel.Code : null,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StatusCode = x.Status.Code ?? string.Empty,
            PriorityId = x.PriorityId,
            PriorityName = x.Priority != null ? x.Priority.Name : null,
            Subject = x.Subject,
            Message = x.Message,
            ActionUrl = x.ActionUrl,
            RelatedEntityType = x.RelatedEntityType,
            RelatedEntityId = x.RelatedEntityId,
            SentAt = x.SentAt,
            ReadAt = x.ReadAt,
            IsDismissed = x.IsDismissed,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private async Task<IReadOnlyCollection<LookupOptionDto>> GetLookupOptionsAsync(string categoryCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync();
    }
}
