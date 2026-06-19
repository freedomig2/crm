using CRM.Domain.Common;

namespace backend.Entities;

public class OpportunityStageHistory : BaseEntity
{
    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = default!;
    public Guid? PreviousStageId { get; set; }
    public LookupValue? PreviousStage { get; set; }
    public Guid NewStageId { get; set; }
    public LookupValue NewStage { get; set; } = default!;
    public Guid? ChangedByUserId { get; set; }
    public AppUser? ChangedByUser { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }
}
