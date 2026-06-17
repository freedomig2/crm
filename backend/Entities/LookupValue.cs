namespace backend.Entities;

public class LookupValue : AuditableEntity
{
    public Guid LookupCategoryId { get; set; }
    public LookupCategory LookupCategory { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
