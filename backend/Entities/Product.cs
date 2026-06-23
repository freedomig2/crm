using CRM.Domain.Common;

namespace backend.Entities;

public class Product : OwnedEntity
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProductCategoryId { get; set; }
    public ProductCategory ProductCategory { get; set; } = default!;
    public Guid ProductTypeId { get; set; }
    public LookupValue ProductType { get; set; } = default!;
    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = default!;
    public Guid ProductStatusId { get; set; }
    public LookupValue ProductStatus { get; set; } = default!;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Brand { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? StandardPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public bool IsStockItem { get; set; }
    public bool AllowDiscount { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<PriceListItem> PriceListItems { get; set; } = new List<PriceListItem>();
    public ICollection<ProductBundleItem> ProductBundleItems { get; set; } = new List<ProductBundleItem>();
}
