using CRM.Domain.Common;

namespace backend.Entities;

public class QuoteLine : BaseEntity
{
    public Guid QuoteId { get; set; }
    public Quote Quote { get; set; } = default!;
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? ProductBundleId { get; set; }
    public ProductBundle? ProductBundle { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}
