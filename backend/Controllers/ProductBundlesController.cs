using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/product-bundles")]
public class ProductBundlesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public ProductBundlesController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("ProductBundles.View")]
    public async Task<ActionResult<PagedResult<ProductBundleDto>>> GetProductBundles([FromQuery] ProductBundleFilterDto query)
    {
        var bundles = _dbContext.ProductBundles.AsQueryable();

        if (query.IsActive.HasValue)
        {
            bundles = bundles.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            bundles = bundles.Where(x =>
                x.BundleCode.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        bundles = bundles.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            bundles = bundles.OrderBy(x => x.Name);
        }

        return Ok(await ProjectBundles(bundles).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("ProductBundles.View")]
    public async Task<ActionResult<ProductBundleDto>> GetProductBundle(Guid id)
    {
        var item = await ProjectBundles(_dbContext.ProductBundles.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("ProductBundles.Create")]
    public async Task<ActionResult<ProductBundleDto>> CreateProductBundle(UpsertProductBundleRequestDto dto)
    {
        var validationError = ValidateBundle(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new ProductBundle();
        ApplyValues(item, dto);

        if (string.IsNullOrWhiteSpace(item.BundleCode))
        {
            item.BundleCode = await _numberSequenceService.GenerateNextAsync("PRODUCT_BUNDLE");
        }

        if (await _dbContext.ProductBundles.AnyAsync(x => x.BundleCode == item.BundleCode))
        {
            return BadRequest("Bundle code already exists.");
        }

        _dbContext.ProductBundles.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectBundles(_dbContext.ProductBundles.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Product bundle was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("ProductBundles.Update")]
    public async Task<IActionResult> UpdateProductBundle(Guid id, UpsertProductBundleRequestDto dto)
    {
        var item = await _dbContext.ProductBundles.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = ValidateBundle(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyValues(item, dto);

        if (string.IsNullOrWhiteSpace(item.BundleCode))
        {
            item.BundleCode = await _numberSequenceService.GenerateNextAsync("PRODUCT_BUNDLE");
        }

        if (await _dbContext.ProductBundles.AnyAsync(x => x.Id != id && x.BundleCode == item.BundleCode))
        {
            return BadRequest("Bundle code already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("ProductBundles.Delete")]
    public async Task<IActionResult> DeleteProductBundle(Guid id)
    {
        var item = await _dbContext.ProductBundles.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{bundleId:guid}/items")]
    [HasPermission("ProductBundles.View")]
    public async Task<ActionResult<PagedResult<ProductBundleItemDto>>> GetBundleItems(Guid bundleId, [FromQuery] ListQueryDto query)
    {
        if (!await _dbContext.ProductBundles.AnyAsync(x => x.Id == bundleId))
        {
            return NotFound();
        }

        var items = _dbContext.ProductBundleItems.Where(x => x.ProductBundleId == bundleId);
        items = items.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            items = items.OrderBy(x => x.SortOrder).ThenBy(x => x.Product.Name);
        }

        return Ok(await ProjectBundleItems(items).ToPagedAsync(query));
    }

    [HttpPost("{bundleId:guid}/items")]
    [HasPermission("ProductBundles.Update")]
    public async Task<ActionResult<ProductBundleItemDto>> AddBundleItem(Guid bundleId, UpsertProductBundleItemRequestDto dto)
    {
        if (!await _dbContext.ProductBundles.AnyAsync(x => x.Id == bundleId))
        {
            return NotFound();
        }

        var validationError = await ValidateBundleItemAsync(null, bundleId, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new ProductBundleItem { ProductBundleId = bundleId };
        ApplyItemValues(item, dto);

        _dbContext.ProductBundleItems.Add(item);
        await _dbContext.SaveChangesAsync();
        await RecalculateBundlePriceAsync(bundleId);

        var created = await ProjectBundleItems(_dbContext.ProductBundleItems.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Bundle item was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("api/product-bundle-items/{id:guid}")]
    [HasPermission("ProductBundles.Update")]
    public async Task<IActionResult> UpdateBundleItem(Guid id, UpsertProductBundleItemRequestDto dto)
    {
        var item = await _dbContext.ProductBundleItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = await ValidateBundleItemAsync(id, item.ProductBundleId, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyItemValues(item, dto);
        await _dbContext.SaveChangesAsync();
        await RecalculateBundlePriceAsync(item.ProductBundleId);

        return NoContent();
    }

    [HttpDelete("api/product-bundle-items/{id:guid}")]
    [HasPermission("ProductBundles.Delete")]
    public async Task<IActionResult> DeleteBundleItem(Guid id)
    {
        var item = await _dbContext.ProductBundleItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var bundleId = item.ProductBundleId;
        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await RecalculateBundlePriceAsync(bundleId);

        return NoContent();
    }

    private static string? ValidateBundle(UpsertProductBundleRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (dto.BundlePrice < 0)
        {
            return "Bundle price cannot be negative.";
        }

        return null;
    }

    private async Task<string?> ValidateBundleItemAsync(Guid? id, Guid bundleId, UpsertProductBundleItemRequestDto dto)
    {
        if (!await _dbContext.Products.AnyAsync(x => x.Id == dto.ProductId))
        {
            return "Product is invalid.";
        }

        if (dto.Quantity <= 0)
        {
            return "Quantity must be greater than zero.";
        }

        var duplicate = await _dbContext.ProductBundleItems.AnyAsync(x =>
            x.Id != id &&
            x.ProductBundleId == bundleId &&
            x.ProductId == dto.ProductId);

        if (duplicate)
        {
            return "Product already exists in this bundle.";
        }

        return null;
    }

    private async Task RecalculateBundlePriceAsync(Guid bundleId)
    {
        var bundle = await _dbContext.ProductBundles.FirstOrDefaultAsync(x => x.Id == bundleId);
        if (bundle is null || bundle.AllowComponentOverride)
        {
            return;
        }

        var total = await _dbContext.ProductBundleItems
            .Where(x => x.ProductBundleId == bundleId)
            .SumAsync(x => x.Quantity * (x.Product.StandardPrice ?? 0m));

        bundle.BundlePrice = total;
        await _dbContext.SaveChangesAsync();
    }

    private static void ApplyValues(ProductBundle item, UpsertProductBundleRequestDto dto)
    {
        item.BundleCode = TrimToNull(dto.BundleCode) ?? string.Empty;
        item.Name = dto.Name.Trim();
        item.Description = TrimToNull(dto.Description);
        item.BundlePrice = dto.BundlePrice;
        item.AllowComponentOverride = dto.AllowComponentOverride;
        item.IsActive = dto.IsActive;
    }

    private static void ApplyItemValues(ProductBundleItem item, UpsertProductBundleItemRequestDto dto)
    {
        item.ProductId = dto.ProductId;
        item.Quantity = dto.Quantity;
        item.SortOrder = dto.SortOrder;
    }

    private static IQueryable<ProductBundleDto> ProjectBundles(IQueryable<ProductBundle> query)
    {
        return query.Select(x => new ProductBundleDto
        {
            Id = x.Id,
            BundleCode = x.BundleCode,
            Name = x.Name,
            Description = x.Description,
            BundlePrice = x.BundlePrice,
            AllowComponentOverride = x.AllowComponentOverride,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static IQueryable<ProductBundleItemDto> ProjectBundleItems(IQueryable<ProductBundleItem> query)
    {
        return query.Select(x => new ProductBundleItemDto
        {
            Id = x.Id,
            ProductBundleId = x.ProductBundleId,
            ProductId = x.ProductId,
            ProductCode = x.Product.ProductCode,
            ProductName = x.Product.Name,
            Quantity = x.Quantity,
            SortOrder = x.SortOrder,
            UnitPrice = x.Product.StandardPrice ?? 0m,
            LineTotal = (x.Product.StandardPrice ?? 0m) * x.Quantity,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
