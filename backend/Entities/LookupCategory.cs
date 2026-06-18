using CRM.Domain.Common;

namespace backend.Entities;

public class LookupCategory : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<LookupValue> Values { get; set; } = new List<LookupValue>();
}
