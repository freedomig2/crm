using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class ActivityDto
{
    public Guid Id { get; set; }
    public string ActivityNumber { get; set; } = string.Empty;
    public Guid ActivityTypeId { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string ActivityTypeCode { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public Guid? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid? AccountId { get; set; }
    public string? AccountName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public Guid? LeadId { get; set; }
    public string? LeadTopic { get; set; }
    public Guid? OpportunityId { get; set; }
    public string? OpportunityTopic { get; set; }
    public Guid? CaseId { get; set; }
    public string? CaseNumber { get; set; }
    public bool IsPrivate { get; set; }
    public Guid? OutcomeId { get; set; }
    public string? OutcomeName { get; set; }
    public DateTime? ReminderAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ActivityCommentDto
{
    public Guid Id { get; set; }
    public Guid ActivityId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
}

public class ActivityFilterDto : ListQueryDto
{
    public Guid? ActivityTypeId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? CaseId { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ActivityDateFrom { get; set; }
    public DateTime? ActivityDateTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}

public class ActivityCommentFilterDto : ListQueryDto
{
    public Guid? ActivityId { get; set; }
}

public class UpsertActivityRequestDto
{
    public string ActivityNumber { get; set; } = string.Empty;

    [Required]
    public Guid ActivityTypeId { get; set; }

    [Required]
    public Guid StatusId { get; set; }

    public Guid? PriorityId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime? ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? CaseId { get; set; }
    public bool IsPrivate { get; set; }
    public Guid? OutcomeId { get; set; }
    public DateTime? ReminderAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class AddActivityCommentRequestDto
{
    [Required]
    public string CommentText { get; set; } = string.Empty;

    public bool IsInternal { get; set; }
}

public class CompleteActivityRequestDto
{
    public DateTime? CompletedDate { get; set; }
    public Guid? StatusId { get; set; }
}

public class ActivityLookupDto
{
    public IReadOnlyCollection<LookupOptionDto> ActivityTypes { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ActivityStatuses { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Priorities { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Outcomes { get; set; } = Array.Empty<LookupOptionDto>();
}
