using CRM.Domain.Common;

namespace backend.Entities;

public class Invoice : OwnedEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Guid CurrencyId { get; set; }
    public LookupValue Currency { get; set; } = default!;
    public Guid InvoiceStatusId { get; set; }
    public LookupValue InvoiceStatus { get; set; } = default!;
    public Guid PaymentStatusId { get; set; }
    public LookupValue PaymentStatus { get; set; } = default!;
    public DateTime? DueDate { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}