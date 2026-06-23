using CRM.Domain.Common;

namespace backend.Entities;

public class SecurityPolicy : ActivatableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid ScopeTypeId { get; set; }
    public LookupValue ScopeType { get; set; } = default!;
    public bool MaskSensitiveFields { get; set; }
    public string? SensitiveFieldList { get; set; }
    public string? Description { get; set; }
}
