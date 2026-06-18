using CRM.Domain.Common;

namespace backend.Entities;

public class AccountAddress : ActivatableEntity
{
    public Guid AccountId { get; set; }
    public Guid? AddressTypeId { get; set; }
    public string? AttentionTo { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Landmark { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public Guid? CountryId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsBilling { get; set; }
    public bool IsShipping { get; set; }
}
