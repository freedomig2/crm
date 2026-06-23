using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public Guid ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public Guid ProductStatusId { get; set; }
    public string? ProductStatusName { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Brand { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? StandardPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public bool IsStockItem { get; set; }
    public bool AllowDiscount { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertProductRequestDto
{
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid ProductCategoryId { get; set; }

    [Required]
    public Guid ProductTypeId { get; set; }

    [Required]
    public Guid UnitOfMeasureId { get; set; }

    [Required]
    public Guid ProductStatusId { get; set; }

    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Brand { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? StandardPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public bool IsStockItem { get; set; }
    public bool AllowDiscount { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class ProductFilterDto : ListQueryDto
{
    public Guid? ProductCategoryId { get; set; }
    public Guid? ProductTypeId { get; set; }
    public Guid? ProductStatusId { get; set; }
    public bool? IsActive { get; set; }
}

public class ProductLookupDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public decimal? StandardPrice { get; set; }
    public decimal? TaxRate { get; set; }
}

public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int SortOrder { get; set; }
    public int ProductCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertProductCategoryRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductCategoryFilterDto : ListQueryDto
{
    public Guid? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public class PriceListDto
{
    public Guid Id { get; set; }
    public string PriceListNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertPriceListRequestDto
{
    public string PriceListNumber { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid CurrencyId { get; set; }

    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PriceListFilterDto : ListQueryDto
{
    public Guid? CurrencyId { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}

public class PriceListItemDto
{
    public Guid Id { get; set; }
    public Guid PriceListId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertPriceListItemRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class PriceListItemFilterDto : ListQueryDto
{
    public Guid? PriceListId { get; set; }
    public Guid? ProductId { get; set; }
}

public class ProductBundleDto
{
    public Guid Id { get; set; }
    public string BundleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BundlePrice { get; set; }
    public bool AllowComponentOverride { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertProductBundleRequestDto
{
    public string BundleCode { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public decimal? BundlePrice { get; set; }
    public bool AllowComponentOverride { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductBundleFilterDto : ListQueryDto
{
    public bool? IsActive { get; set; }
}

public class ProductBundleItemDto
{
    public Guid Id { get; set; }
    public Guid ProductBundleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int SortOrder { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertProductBundleItemRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }
    public int SortOrder { get; set; }
}

public class UnitOfMeasureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertUnitOfMeasureRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UnitOfMeasureFilterDto : ListQueryDto
{
    public bool? IsActive { get; set; }
}

public class DiscountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid DiscountTypeId { get; set; }
    public string? DiscountTypeName { get; set; }
    public decimal Value { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool IsStackable { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertDiscountRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public Guid DiscountTypeId { get; set; }

    public decimal Value { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool IsStackable { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Description { get; set; }
}

public class DiscountFilterDto : ListQueryDto
{
    public Guid? DiscountTypeId { get; set; }
    public bool? IsActive { get; set; }
}
