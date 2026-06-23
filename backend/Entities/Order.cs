using CRM.Domain.Common;

namespace backend.Entities;

public class Order : OwnedEntity
{
    public string OrderNumber { get; set; } = string.Empty;
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
    public Guid OrderStatusId { get; set; }
    public LookupValue OrderStatus { get; set; } = default!;
    public Guid ApprovalStatusId { get; set; }
    public LookupValue ApprovalStatus { get; set; } = default!;
    public Guid DeliveryStatusId { get; set; }
    public LookupValue DeliveryStatus { get; set; } = default!;
    public Guid BillingStatusId { get; set; }
    public LookupValue BillingStatus { get; set; } = default!;
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? BillingDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? ApprovedById { get; set; }
    public AppUser? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ConvertedInvoiceId { get; set; }
    public DateTime? ConvertedInvoiceAt { get; set; }
    public AppUser? OwnerUser { get; set; }
    public Team? OwnerTeam { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
