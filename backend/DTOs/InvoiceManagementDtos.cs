using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
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
    public Guid InvoiceStatusId { get; set; }
    public string InvoiceStatusName { get; set; } = string.Empty;
    public Guid PaymentStatusId { get; set; }
    public string PaymentStatusName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
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

public class UpsertInvoiceRequestDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Guid? QuoteId { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactId { get; set; }
    public Guid? OpportunityId { get; set; }

    [Required]
    public Guid CurrencyId { get; set; }

    [Required]
    public Guid InvoiceStatusId { get; set; }

    [Required]
    public Guid PaymentStatusId { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class MarkInvoicePaidRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal PaidAmount { get; set; }

    public DateTime? PaidDate { get; set; }
}

public class UpsertInvoiceLineRequestDto
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

public class InvoiceFilterDto : ListQueryDto
{
    public Guid? AccountId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? InvoiceStatusId { get; set; }
    public Guid? PaymentStatusId { get; set; }
    public bool? IsActive { get; set; }
}

public class InvoiceLineFilterDto : ListQueryDto
{
    public Guid? InvoiceId { get; set; }
}