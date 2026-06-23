using CRM.Domain.Common;

namespace backend.Entities;

public class ProductBundle : ActivatableEntity
{
    public string BundleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BundlePrice { get; set; }
    public bool AllowComponentOverride { get; set; }
    public ICollection<ProductBundleItem> Items { get; set; } = new List<ProductBundleItem>();
}
