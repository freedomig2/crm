using CRM.Domain.Common;

namespace backend.Entities;

public class Account : OwnedEntity
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradingName { get; set; }
    public Guid? AccountTypeId { get; set; }
    public Guid? IndustryId { get; set; }
    public Guid? OwnershipTypeId { get; set; }
    public Guid? CustomerStatusId { get; set; }
    public Guid? CustomerSegmentId { get; set; }
    public string? Website { get; set; }
    public string? MainPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Fax { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? Description { get; set; }
    public Guid? ParentAccountId { get; set; }
    public Guid? PrimaryContactId { get; set; }
}
