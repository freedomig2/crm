using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class OpportunityDto
{
    public Guid Id { get; set; }
    public string OpportunityNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public Guid? LeadId { get; set; }
    public string? LeadTopic { get; set; }
    public Guid OpportunityStageId { get; set; }
    public string? OpportunityStageName { get; set; }
    public string? OpportunityStageCode { get; set; }
    public Guid OpportunityStatusId { get; set; }
    public string? OpportunityStatusName { get; set; }
    public string? OpportunityStatusCode { get; set; }
    public Guid? SalesProcessStageId { get; set; }
    public string? SalesProcessStageName { get; set; }
    public Guid? RatingId { get; set; }
    public string? RatingName { get; set; }
    public Guid? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public decimal Probability { get; set; }
    public decimal? WeightedRevenue { get; set; }
    public decimal? ActualRevenue { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public Guid? CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    public Guid? WinReasonId { get; set; }
    public string? WinReasonName { get; set; }
    public Guid? LossReasonId { get; set; }
    public string? LossReasonName { get; set; }
    public Guid? LostToCompetitorId { get; set; }
    public string? LostToCompetitorName { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? OwnerUserName { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public string? OwnerTeamName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertOpportunityRequestDto
{
    public string OpportunityNumber { get; set; } = string.Empty;

    [Required]
    public string Topic { get; set; } = string.Empty;

    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid OpportunityStageId { get; set; }
    public Guid OpportunityStatusId { get; set; }
    public Guid? SalesProcessStageId { get; set; }
    public Guid? RatingId { get; set; }
    public Guid? PriorityId { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public decimal Probability { get; set; }
    public decimal? ActualRevenue { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public Guid? CurrencyId { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? WinReasonId { get; set; }
    public Guid? LossReasonId { get; set; }
    public Guid? LostToCompetitorId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class OpportunityFilterDto : ListQueryDto
{
    public Guid? AccountId { get; set; }
    public Guid? OpportunityStageId { get; set; }
    public Guid? OpportunityStatusId { get; set; }
    public Guid? RatingId { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime? EstimatedCloseFrom { get; set; }
    public DateTime? EstimatedCloseTo { get; set; }
    public decimal? MinRevenue { get; set; }
    public decimal? MaxRevenue { get; set; }
    public bool? IsActive { get; set; }
}

public class OpportunityAssignOwnerRequestDto
{
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class OpportunityChangeStageRequestDto
{
    [Required]
    public Guid OpportunityStageId { get; set; }
}

public class OpportunityMarkWonRequestDto
{
    [Required]
    public decimal? ActualRevenue { get; set; }
    [Required]
    public DateTime? ActualCloseDate { get; set; }
    [Required]
    public Guid? WinReasonId { get; set; }
    public string? Notes { get; set; }
}

public class OpportunityMarkLostRequestDto
{
    [Required]
    public DateTime? ActualCloseDate { get; set; }
    [Required]
    public Guid? LossReasonId { get; set; }
    public Guid? LostToCompetitorId { get; set; }
    public string? Notes { get; set; }
}

public class OpportunitySummaryDto
{
    public OpportunityDto Opportunity { get; set; } = default!;
    public OpportunityActivityDto? LatestActivity { get; set; }
    public OpportunityCompetitorDto? PrimaryCompetitor { get; set; }
    public decimal ProductRevenue { get; set; }
    public int ProductCount { get; set; }
    public int CompetitorCount { get; set; }
    public int ActivityCount { get; set; }
}

public class OpportunityPipelineStageDto
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? StageCode { get; set; }
    public int Count { get; set; }
    public decimal EstimatedRevenue { get; set; }
    public decimal WeightedRevenue { get; set; }
    public IReadOnlyCollection<OpportunityDto> Opportunities { get; set; } = Array.Empty<OpportunityDto>();
}

public class OpportunityTimelineItemDto
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

public class OpportunityProductDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}

public class OpportunityProductFilterDto : ListQueryDto
{
    public Guid? OpportunityId { get; set; }
}

public class UpsertOpportunityProductRequestDto
{
    public Guid? ProductId { get; set; }
    [Required]
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public int SortOrder { get; set; }
}

public class OpportunityCompetitorDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string CompetitorName { get; set; } = string.Empty;
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public Guid? ThreatLevelId { get; set; }
    public string? ThreatLevelName { get; set; }
    public bool IsPrimaryCompetitor { get; set; }
    public string? Notes { get; set; }
}

public class OpportunityCompetitorFilterDto : ListQueryDto
{
    public Guid? OpportunityId { get; set; }
    public Guid? ThreatLevelId { get; set; }
    public bool? IsPrimaryCompetitor { get; set; }
}

public class UpsertOpportunityCompetitorRequestDto
{
    [Required]
    public string CompetitorName { get; set; } = string.Empty;
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public Guid? ThreatLevelId { get; set; }
    public bool IsPrimaryCompetitor { get; set; }
    public string? Notes { get; set; }
}

public class OpportunityActivityDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
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

public class OpportunityActivityFilterDto : ListQueryDto
{
    public Guid? OpportunityId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public class UpsertOpportunityActivityRequestDto
{
    public Guid? ContactId { get; set; }
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

public class CompleteOpportunityActivityRequestDto
{
    public DateTime? CompletedDate { get; set; }
    public Guid? StatusId { get; set; }
}

public class OpportunityLookupDto
{
    public IReadOnlyCollection<LookupOptionDto> OpportunityStages { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> OpportunityStatuses { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> SalesProcessStages { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Ratings { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Priorities { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Currencies { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> Sources { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> WinReasons { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> LossReasons { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ThreatLevels { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ActivityTypes { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> ActivityStatuses { get; set; } = Array.Empty<LookupOptionDto>();
}

public class OpportunityDashboardSummaryDto
{
    public int TotalOpportunities { get; set; }
    public int OpenOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal WeightedPipelineValue { get; set; }
    public decimal AverageProbability { get; set; }
    public int ClosingThisMonth { get; set; }
    public IReadOnlyCollection<LeadDashboardGroupDto> OpportunitiesByStage { get; set; } = Array.Empty<LeadDashboardGroupDto>();
    public IReadOnlyCollection<LeadDashboardGroupDto> OpportunitiesByOwner { get; set; } = Array.Empty<LeadDashboardGroupDto>();
    public IReadOnlyCollection<OpportunityDto> RecentOpportunities { get; set; } = Array.Empty<OpportunityDto>();
}
