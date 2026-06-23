using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private const string ProductTypeCategoryCode = "PRODUCT_TYPE";
    private const string ProductStatusCategoryCode = "PRODUCT_STATUS";
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public ProductsController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("Products.View")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductFilterDto query)
    {
        var products = _dbContext.Products.AsQueryable();

        if (query.ProductCategoryId.HasValue)
        {
            products = products.Where(x => x.ProductCategoryId == query.ProductCategoryId.Value);
        }

        if (query.ProductTypeId.HasValue)
        {
            products = products.Where(x => x.ProductTypeId == query.ProductTypeId.Value);
        }

        if (query.ProductStatusId.HasValue)
        {
            products = products.Where(x => x.ProductStatusId == query.ProductStatusId.Value);
        }

        if (query.IsActive.HasValue)
        {
            products = products.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            products = products.Where(x =>
                x.ProductCode.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.SKU ?? string.Empty).ToLower().Contains(search) ||
                (x.Barcode ?? string.Empty).ToLower().Contains(search) ||
                (x.ProductCategory != null ? x.ProductCategory.Name : string.Empty).ToLower().Contains(search));
        }

        products = products.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            products = products.OrderBy(x => x.Name);
        }

        return Ok(await ProjectProducts(products).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Products.View")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await ProjectProducts(_dbContext.Products.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("lookup")]
    [HasPermission("Products.View")]
    public async Task<ActionResult<IReadOnlyCollection<ProductLookupDto>>> GetLookup([FromQuery] string? search)
    {
        var query = _dbContext.Products.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLower();
            query = query.Where(x =>
                x.ProductCode.ToLower().Contains(normalized) ||
                x.Name.ToLower().Contains(normalized));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new ProductLookupDto
            {
                Id = x.Id,
                ProductCode = x.ProductCode,
                Name = x.Name,
                UnitOfMeasureId = x.UnitOfMeasureId,
                UnitOfMeasureName = x.UnitOfMeasure.Name,
                StandardPrice = x.StandardPrice,
                TaxRate = x.TaxRate,
            })
            .Take(100)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [HasPermission("Products.Create")]
    public async Task<ActionResult<ProductDto>> CreateProduct(UpsertProductRequestDto dto)
    {
        var validationError = await ValidateProductAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var product = new Product();
        ApplyValues(product, dto);
        if (string.IsNullOrWhiteSpace(product.ProductCode))
        {
            product.ProductCode = await _numberSequenceService.GenerateNextAsync("PRODUCT");
        }

        if (await _dbContext.Products.AnyAsync(x => x.ProductCode == product.ProductCode))
        {
            return BadRequest("Product code already exists.");
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectProducts(_dbContext.Products.Where(x => x.Id == product.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Product was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Products.Update")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpsertProductRequestDto dto)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var validationError = await ValidateProductAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyValues(product, dto);
        if (string.IsNullOrWhiteSpace(product.ProductCode))
        {
            product.ProductCode = await _numberSequenceService.GenerateNextAsync("PRODUCT");
        }

        if (await _dbContext.Products.AnyAsync(x => x.Id != id && x.ProductCode == product.ProductCode))
        {
            return BadRequest("Product code already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Products.Delete")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        product.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateProductAsync(UpsertProductRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (!await _dbContext.ProductCategories.AnyAsync(x => x.Id == dto.ProductCategoryId))
        {
            return "Product category is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ProductTypeId && x.LookupCategory.Code == ProductTypeCategoryCode))
        {
            return "Product type is invalid.";
        }

        if (!await _dbContext.UnitOfMeasures.AnyAsync(x => x.Id == dto.UnitOfMeasureId))
        {
            return "Unit of measure is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ProductStatusId && x.LookupCategory.Code == ProductStatusCategoryCode))
        {
            return "Product status is invalid.";
        }

        if (dto.CostPrice < 0 || dto.StandardPrice < 0 || dto.TaxRate < 0 || dto.Weight < 0 || dto.Volume < 0)
        {
            return "Pricing and measurement values cannot be negative.";
        }

        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom.Value)
        {
            return "Effective To date must be greater than or equal to Effective From date.";
        }

        return null;
    }

    private static void ApplyValues(Product item, UpsertProductRequestDto dto)
    {
        item.ProductCode = TrimToNull(dto.ProductCode) ?? string.Empty;
        item.Name = dto.Name.Trim();
        item.Description = TrimToNull(dto.Description);
        item.ProductCategoryId = dto.ProductCategoryId;
        item.ProductTypeId = dto.ProductTypeId;
        item.UnitOfMeasureId = dto.UnitOfMeasureId;
        item.ProductStatusId = dto.ProductStatusId;
        item.SKU = TrimToNull(dto.SKU);
        item.Barcode = TrimToNull(dto.Barcode);
        item.Manufacturer = TrimToNull(dto.Manufacturer);
        item.Brand = TrimToNull(dto.Brand);
        item.CostPrice = dto.CostPrice;
        item.StandardPrice = dto.StandardPrice;
        item.TaxRate = dto.TaxRate;
        item.Weight = dto.Weight;
        item.Volume = dto.Volume;
        item.IsStockItem = dto.IsStockItem;
        item.AllowDiscount = dto.AllowDiscount;
        item.EffectiveFrom = dto.EffectiveFrom;
        item.EffectiveTo = dto.EffectiveTo;
        item.IsActive = dto.IsActive;
        item.OwnerUserId = dto.OwnerUserId;
        item.OwnerTeamId = dto.OwnerTeamId;
    }

    private static IQueryable<ProductDto> ProjectProducts(IQueryable<Product> query)
    {
        return query.Select(x => new ProductDto
        {
            Id = x.Id,
            ProductCode = x.ProductCode,
            Name = x.Name,
            Description = x.Description,
            ProductCategoryId = x.ProductCategoryId,
            ProductCategoryName = x.ProductCategory.Name,
            ProductTypeId = x.ProductTypeId,
            ProductTypeName = x.ProductType.Name,
            UnitOfMeasureId = x.UnitOfMeasureId,
            UnitOfMeasureName = x.UnitOfMeasure.Name,
            ProductStatusId = x.ProductStatusId,
            ProductStatusName = x.ProductStatus.Name,
            SKU = x.SKU,
            Barcode = x.Barcode,
            Manufacturer = x.Manufacturer,
            Brand = x.Brand,
            CostPrice = x.CostPrice,
            StandardPrice = x.StandardPrice,
            TaxRate = x.TaxRate,
            Weight = x.Weight,
            Volume = x.Volume,
            IsStockItem = x.IsStockItem,
            AllowDiscount = x.AllowDiscount,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
