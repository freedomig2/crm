using CRM.Domain.Common;

namespace backend.Entities;

public class OpportunityActivity : BaseEntity
{
    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = default!;
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid ActivityTypeId { get; set; }
    public LookupValue ActivityType { get; set; } = default!;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid StatusId { get; set; }
    public LookupValue Status { get; set; } = default!;
    public Guid? PriorityId { get; set; }
    public LookupValue? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
}
