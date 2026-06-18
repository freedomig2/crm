using CRM.Domain.Common;

namespace backend.Entities;

public class AccountRelationship : ActivatableEntity
{
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public Guid? RelationshipTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? StrengthId { get; set; }
    public string? Notes { get; set; }
}
