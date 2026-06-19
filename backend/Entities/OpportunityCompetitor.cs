using CRM.Domain.Common;

namespace backend.Entities;

public class OpportunityCompetitor : BaseEntity
{
    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = default!;
    public string CompetitorName { get; set; } = string.Empty;
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public Guid? ThreatLevelId { get; set; }
    public LookupValue? ThreatLevel { get; set; }
    public bool IsPrimaryCompetitor { get; set; }
    public string? Notes { get; set; }
}
