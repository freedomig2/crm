using CRM.Domain.Common;

namespace backend.Entities;

public class Quote : OwnedEntity
{
    public string QuoteNumber { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Guid PriceListId { get; set; }
    public PriceList PriceList { get; set; } = default!;
    public Guid CurrencyId { get; set; }
    public LookupValue Currency { get; set; } = default!;
    public Guid QuoteStatusId { get; set; }
    public LookupValue QuoteStatus { get; set; } = default!;
    public Guid ApprovalStatusId { get; set; }
    public LookupValue ApprovalStatus { get; set; } = default!;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public Guid? ApprovedById { get; set; }
    public AppUser? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ConvertedOrderId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<QuoteLine> Lines { get; set; } = new List<QuoteLine>();
}
