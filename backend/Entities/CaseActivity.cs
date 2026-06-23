using CRM.Domain.Common;

namespace backend.Entities;

public class CaseActivity : BaseEntity
{
    public Guid CaseId { get; set; }
    public ServiceCase Case { get; set; } = default!;
    public Guid? ActivityTypeId { get; set; }
    public LookupValue? ActivityType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public Guid? StatusId { get; set; }
    public LookupValue? Status { get; set; }
    public Guid? PriorityId { get; set; }
    public LookupValue? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
}
