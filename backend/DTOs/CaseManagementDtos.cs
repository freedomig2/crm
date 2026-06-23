using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public Guid? OpportunityId { get; set; }
    public string? OpportunityTopic { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CaseStatusId { get; set; }
    public string CaseStatusName { get; set; } = string.Empty;
    public string CaseStatusCode { get; set; } = string.Empty;
    public Guid PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public Guid? SeverityId { get; set; }
    public string? SeverityName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid? EscalatedToUserId { get; set; }
    public string? EscalatedToUserName { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionSummary { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CaseCommentDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
}

public class UpsertCaseRequestDto
{
    public string CaseNumber { get; set; } = string.Empty;

    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactId { get; set; }
    public Guid? OpportunityId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid CaseStatusId { get; set; }

    [Required]
    public Guid PriorityId { get; set; }

    public Guid? SeverityId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? EscalatedToUserId { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionSummary { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class ResolveCaseRequestDto
{
    public string? ResolutionSummary { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CloseCaseRequestDto
{
    public DateTime? ClosedAt { get; set; }
}

public class ReopenCaseRequestDto
{
    public DateTime? ReopenedAt { get; set; }
}

public class AddCaseCommentRequestDto
{
    [Required]
    public string CommentText { get; set; } = string.Empty;

    public bool IsInternal { get; set; }
}

public class CaseFilterDto : ListQueryDto
{
    public Guid? AccountId { get; set; }
    public Guid? CaseStatusId { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public bool? IsActive { get; set; }
}

public class CaseCommentFilterDto : ListQueryDto
{
    public Guid? CaseId { get; set; }
}
