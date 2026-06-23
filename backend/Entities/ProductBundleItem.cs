using CRM.Domain.Common;

namespace backend.Entities;

public class ProductBundleItem : BaseEntity
{
    public Guid ProductBundleId { get; set; }
    public ProductBundle ProductBundle { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public decimal Quantity { get; set; }
    public int SortOrder { get; set; }
}
