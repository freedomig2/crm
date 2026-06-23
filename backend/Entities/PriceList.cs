using CRM.Domain.Common;

namespace backend.Entities;

public class PriceList : ActivatableEntity
{
    public string PriceListNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CurrencyId { get; set; }
    public LookupValue Currency { get; set; } = default!;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
}
