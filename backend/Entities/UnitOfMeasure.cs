using CRM.Domain.Common;

namespace backend.Entities;

public class UnitOfMeasure : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
