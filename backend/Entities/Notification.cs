using CRM.Domain.Common;

namespace backend.Entities;

public class Notification : ActivatableEntity
{
    public Guid RecipientUserId { get; set; }
    public AppUser RecipientUser { get; set; } = default!;
    public Guid? NotificationTemplateId { get; set; }
    public NotificationTemplate? NotificationTemplate { get; set; }
    public Guid? ChannelId { get; set; }
    public LookupValue? Channel { get; set; }
    public Guid StatusId { get; set; }
    public LookupValue Status { get; set; } = default!;
    public Guid? PriorityId { get; set; }
    public LookupValue? Priority { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDismissed { get; set; }
}
