using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? QuoteId { get; set; }
    public string? QuoteNumber { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public Guid? OpportunityId { get; set; }
    public string? OpportunityTopic { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyName { get; set; } = string.Empty;
    public Guid OrderStatusId { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;
    public Guid ApprovalStatusId { get; set; }
    public string ApprovalStatusName { get; set; } = string.Empty;
    public Guid DeliveryStatusId { get; set; }
    public string DeliveryStatusName { get; set; } = string.Empty;
    public Guid BillingStatusId { get; set; }
    public string BillingStatusName { get; set; } = string.Empty;
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
    public DateTime? ApprovedAt { get; set; }
    public Guid? ConvertedInvoiceId { get; set; }
    public DateTime? ConvertedInvoiceAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderLineDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductBundleId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertOrderRequestDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? QuoteId { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactId { get; set; }
    public Guid? OpportunityId { get; set; }

    [Required]
    public Guid CurrencyId { get; set; }

    [Required]
    public Guid OrderStatusId { get; set; }

    [Required]
    public Guid ApprovalStatusId { get; set; }

    [Required]
    public Guid DeliveryStatusId { get; set; }

    [Required]
    public Guid BillingStatusId { get; set; }

    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? BillingDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class UpsertOrderLineRequestDto
{
    public Guid? ProductId { get; set; }
    public Guid? ProductBundleId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxRate { get; set; }
    public int SortOrder { get; set; }
}

public class OrderFilterDto : ListQueryDto
{
    public Guid? AccountId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? OrderStatusId { get; set; }
    public Guid? ApprovalStatusId { get; set; }
    public Guid? DeliveryStatusId { get; set; }
    public Guid? BillingStatusId { get; set; }
    public bool? IsActive { get; set; }
}

public class OrderLineFilterDto : ListQueryDto
{
    public Guid? OrderId { get; set; }
}
