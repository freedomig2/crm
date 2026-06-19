using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public string LeadNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? Website { get; set; }
    public Guid? LeadSourceId { get; set; }
    public string? LeadSourceName { get; set; }
    public Guid LeadStatusId { get; set; }
    public string? LeadStatusName { get; set; }
    public Guid? QualificationStatusId { get; set; }
    public string? QualificationStatusName { get; set; }
    public Guid? RatingId { get; set; }
    public string? RatingName { get; set; }
    public Guid? IndustryId { get; set; }
    public string? IndustryName { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public int Score { get; set; }
    public string? ScoreGrade { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public Guid? AssignedToTeamId { get; set; }
    public string? AssignedToTeamName { get; set; }
    public Guid? ConvertedAccountId { get; set; }
    public string? ConvertedAccountName { get; set; }
    public Guid? ConvertedContactId { get; set; }
    public string? ConvertedContactName { get; set; }
    public Guid? ConvertedOpportunityId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public Guid? ConvertedById { get; set; }
    public string? ConvertedByName { get; set; }
    public Guid? DisqualifiedReasonId { get; set; }
    public string? DisqualifiedReasonName { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? OwnerUserName { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public string? OwnerTeamName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpsertLeadRequestDto
{
    public string LeadNumber { get; set; } = string.Empty;

    [Required]
    public string Topic { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? Website { get; set; }
    public Guid? LeadSourceId { get; set; }
    public Guid LeadStatusId { get; set; }
    public Guid? QualificationStatusId { get; set; }
    public Guid? RatingId { get; set; }
    public Guid? IndustryId { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? AssignedToTeamId { get; set; }
    public Guid? DisqualifiedReasonId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LeadFilterDto : ListQueryDto
{
    public Guid? LeadSourceId { get; set; }
    public Guid? LeadStatusId { get; set; }
    public Guid? QualificationStatusId { get; set; }
    public Guid? RatingId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public bool? IsConverted { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

public class LeadAssignRequestDto
{
    public Guid? AssignedToUserId { get; set; }
    public Guid? AssignedToTeamId { get; set; }
}

public class LeadDisqualifyRequestDto
{
    [Required]
    public Guid DisqualifiedReasonId { get; set; }
}

public class LeadConversionRequestDto
{
    public bool CreateAccount { get; set; }
    public Guid? ExistingAccountId { get; set; }
    public bool CreateContact { get; set; }
    public Guid? ExistingContactId { get; set; }
    public bool CreateOpportunity { get; set; }
    public string? OpportunityTopic { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
}

public class LeadConversionResultDto
{
    public Guid LeadId { get; set; }
    public Guid ConvertedAccountId { get; set; }
    public string? ConvertedAccountName { get; set; }
    public Guid ConvertedContactId { get; set; }
    public string? ConvertedContactName { get; set; }
    public Guid? ConvertedOpportunityId { get; set; }
    public string? OpportunityMessage { get; set; }
}

public class LeadTimelineItemDto
{
    public Guid Id { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? AssignedToName { get; set; }
}

public class LeadActivityDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string? LeadTopic { get; set; }
    public Guid ActivityTypeId { get; set; }
    public string? ActivityTypeName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid StatusId { get; set; }
    public string? StatusName { get; set; }
    public Guid? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
}

public class UpsertLeadActivityRequestDto
{
    [Required]
    public Guid LeadId { get; set; }

    [Required]
    public Guid ActivityTypeId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    [Required]
    public Guid StatusId { get; set; }

    public Guid? PriorityId { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class LeadActivityFilterDto : ListQueryDto
{
    public Guid? LeadId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class CompleteLeadActivityRequestDto
{
    public DateTime? CompletedDate { get; set; }
    public Guid? StatusId { get; set; }
}

public class LeadScoreRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid RuleTypeId { get; set; }
    public string? RuleTypeName { get; set; }
    public string? FieldName { get; set; }
    public string? Operator { get; set; }
    public string? CompareValue { get; set; }
    public int ScoreValue { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertLeadScoreRuleRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid RuleTypeId { get; set; }

    public string? FieldName { get; set; }
    public string? Operator { get; set; }
    public string? CompareValue { get; set; }
    public int ScoreValue { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LeadScoreRuleFilterDto : ListQueryDto
{
    public Guid? RuleTypeId { get; set; }
    public bool? IsActive { get; set; }
}

public class LeadScoreRuleRunResultDto
{
    public int RecalculatedLeads { get; set; }
}

public class LookupOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class LeadLookupDto
{
    public IReadOnlyCollection<LookupOptionDto> LeadSources { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> LeadStatuses { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> QualificationStatuses { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Ratings { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Industries { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> DisqualificationReasons { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ActivityTypes { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ActivityStatuses { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Priorities { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ScoreRuleTypes { get; set; } = Array.Empty<LookupOptionDto>();
}

public class LeadDashboardSummaryDto
{
    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int QualifiedLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public int DisqualifiedLeads { get; set; }
    public double AverageLeadScore { get; set; }
    public int HotLeads { get; set; }
    public IReadOnlyCollection<LeadDashboardGroupDto> LeadsBySource { get; set; } = Array.Empty<LeadDashboardGroupDto>();
    public IReadOnlyCollection<LeadDashboardGroupDto> LeadsByStatus { get; set; } = Array.Empty<LeadDashboardGroupDto>();
    public IReadOnlyCollection<LeadDashboardItemDto> RecentLeads { get; set; } = Array.Empty<LeadDashboardItemDto>();
    public IReadOnlyCollection<LeadDashboardItemDto> RecentlyConvertedLeads { get; set; } = Array.Empty<LeadDashboardItemDto>();
}

public class LeadDashboardGroupDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LeadDashboardItemDto
{
    public Guid Id { get; set; }
    public string LeadNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? StatusName { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
}
