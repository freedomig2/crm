using CRM.Domain.Common;

namespace backend.Entities;

public class ContactInteraction : BaseEntity
{
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = default!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? InteractionTypeId { get; set; }
    public LookupValue? InteractionType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime InteractionDate { get; set; }
    public string? Outcome { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
}
