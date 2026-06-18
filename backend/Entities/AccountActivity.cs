using CRM.Domain.Common;

namespace backend.Entities;

public class AccountActivity : BaseEntity
{
    public Guid AccountId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? OutcomeId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsPrivate { get; set; }
    public bool FollowUpRequired { get; set; }
    public DateTime? FollowUpDate { get; set; }
}
