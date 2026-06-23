using CRM.Domain.Common;

namespace backend.Entities;

public class Activity : OwnedEntity
{
    public string ActivityNumber { get; set; } = string.Empty;
    public Guid ActivityTypeId { get; set; }
    public LookupValue ActivityType { get; set; } = default!;
    public Guid StatusId { get; set; }
    public LookupValue Status { get; set; } = default!;
    public Guid? PriorityId { get; set; }
    public LookupValue? Priority { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Guid? CaseId { get; set; }
    public ServiceCase? Case { get; set; }
    public bool IsPrivate { get; set; }
    public Guid? OutcomeId { get; set; }
    public LookupValue? Outcome { get; set; }
    public DateTime? ReminderAt { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<ActivityComment> Comments { get; set; } = new List<ActivityComment>();
}
