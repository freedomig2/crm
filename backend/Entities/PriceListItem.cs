using CRM.Domain.Common;

namespace backend.Entities;

public class PriceListItem : BaseEntity
{
    public Guid PriceListId { get; set; }
    public PriceList PriceList { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
