using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class QuoteDto
{
    public Guid Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public Guid? OpportunityId { get; set; }
    public string? OpportunityTopic { get; set; }
    public Guid PriceListId { get; set; }
    public string PriceListName { get; set; } = string.Empty;
    public Guid CurrencyId { get; set; }
    public string CurrencyName { get; set; } = string.Empty;
    public Guid QuoteStatusId { get; set; }
    public string QuoteStatusName { get; set; } = string.Empty;
    public Guid ApprovalStatusId { get; set; }
    public string ApprovalStatusName { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ConvertedOrderId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class QuoteLineDto
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
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

public class UpsertQuoteRequestDto
{
    public string QuoteNumber { get; set; } = string.Empty;

    [Required]
    public Guid AccountId { get; set; }

    public Guid? ContactId { get; set; }
    public Guid? OpportunityId { get; set; }

    [Required]
    public Guid PriceListId { get; set; }

    [Required]
    public Guid CurrencyId { get; set; }

    [Required]
    public Guid QuoteStatusId { get; set; }

    [Required]
    public Guid ApprovalStatusId { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class UpsertQuoteLineRequestDto
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

public class QuoteFilterDto : ListQueryDto
{
    public Guid? AccountId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? QuoteStatusId { get; set; }
    public Guid? ApprovalStatusId { get; set; }
    public bool? IsActive { get; set; }
}

public class QuoteLineFilterDto : ListQueryDto
{
    public Guid? QuoteId { get; set; }
}
