using CRM.Domain.Common;

namespace backend.Entities;

public class ServiceCase : OwnedEntity
{
    public string CaseNumber { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CaseStatusId { get; set; }
    public LookupValue CaseStatus { get; set; } = default!;
    public Guid PriorityId { get; set; }
    public LookupValue Priority { get; set; } = default!;
    public Guid? SeverityId { get; set; }
    public LookupValue? Severity { get; set; }
    public Guid? CategoryId { get; set; }
    public LookupValue? Category { get; set; }
    public Guid? SourceId { get; set; }
    public LookupValue? Source { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
    public Guid? EscalatedToUserId { get; set; }
    public AppUser? EscalatedToUser { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionSummary { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<CaseComment> Comments { get; set; } = new List<CaseComment>();
    public ICollection<CaseActivity> Activities { get; set; } = new List<CaseActivity>();
}
