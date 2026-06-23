using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelCode { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public string? RecipientUserEmail { get; set; }
    public Guid? NotificationTemplateId { get; set; }
    public string? NotificationTemplateName { get; set; }
    public Guid? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelCode { get; set; }
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public Guid? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDismissed { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class NotificationTemplateFilterDto : ListQueryDto
{
    public Guid? ChannelId { get; set; }
    public bool? IsSystem { get; set; }
    public bool? IsActive { get; set; }
}

public class NotificationFilterDto : ListQueryDto
{
    public Guid? RecipientUserId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? ChannelId { get; set; }
    public Guid? PriorityId { get; set; }
    public bool? IsDismissed { get; set; }
    public bool? IsActive { get; set; }
}

public class UpsertNotificationTemplateRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string SubjectTemplate { get; set; } = string.Empty;

    [Required]
    public string BodyTemplate { get; set; } = string.Empty;

    [Required]
    public Guid ChannelId { get; set; }

    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpsertNotificationRequestDto
{
    [Required]
    public Guid RecipientUserId { get; set; }

    public Guid? NotificationTemplateId { get; set; }
    public Guid? ChannelId { get; set; }

    [Required]
    public Guid StatusId { get; set; }

    public Guid? PriorityId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDismissed { get; set; }
    public bool IsActive { get; set; } = true;
}

public class NotificationLookupDto
{
    public IReadOnlyCollection<LookupOptionDto> Channels { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Statuses { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Priorities { get; set; } = Array.Empty<LookupOptionDto>();
}
