using CRM.Domain.Common;

namespace backend.Entities;

public class Opportunity : OwnedEntity
{
    public string OpportunityNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public Guid OpportunityStageId { get; set; }
    public LookupValue OpportunityStage { get; set; } = default!;
    public Guid OpportunityStatusId { get; set; }
    public LookupValue OpportunityStatus { get; set; } = default!;
    public Guid? SalesProcessStageId { get; set; }
    public LookupValue? SalesProcessStage { get; set; }
    public Guid? RatingId { get; set; }
    public LookupValue? Rating { get; set; }
    public Guid? PriorityId { get; set; }
    public LookupValue? Priority { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public decimal Probability { get; set; }
    public decimal? WeightedRevenue { get; set; }
    public decimal? ActualRevenue { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public Guid? CurrencyId { get; set; }
    public LookupValue? Currency { get; set; }
    public Guid? SourceId { get; set; }
    public LookupValue? Source { get; set; }
    public Guid? WinReasonId { get; set; }
    public LookupValue? WinReason { get; set; }
    public Guid? LossReasonId { get; set; }
    public LookupValue? LossReason { get; set; }
    public Guid? LostToCompetitorId { get; set; }
    public OpportunityCompetitor? LostToCompetitor { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<OpportunityProduct> Products { get; set; } = new List<OpportunityProduct>();
    public ICollection<OpportunityCompetitor> Competitors { get; set; } = new List<OpportunityCompetitor>();
    public ICollection<OpportunityActivity> Activities { get; set; } = new List<OpportunityActivity>();
}
