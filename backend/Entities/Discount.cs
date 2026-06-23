using CRM.Domain.Common;

namespace backend.Entities;

public class Discount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid DiscountTypeId { get; set; }
    public LookupValue DiscountType { get; set; } = default!;
    public decimal Value { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool IsStackable { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Description { get; set; }
}
