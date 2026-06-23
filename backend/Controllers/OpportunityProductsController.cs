using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class OpportunityProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OpportunityProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("api/opportunities/{opportunityId:guid}/products")]
    [HasPermission("OpportunityProducts.View")]
    public async Task<ActionResult<PagedResult<OpportunityProductDto>>> GetOpportunityProducts(Guid opportunityId, [FromQuery] OpportunityProductFilterDto query)
    {
        if (!await _dbContext.Opportunities.AnyAsync(x => x.Id == opportunityId))
        {
            return NotFound();
        }

        query.OpportunityId = opportunityId;
        return await GetProducts(query);
    }

    [HttpGet("api/opportunity-products")]
    [HasPermission("OpportunityProducts.View")]
    public async Task<ActionResult<PagedResult<OpportunityProductDto>>> GetProducts([FromQuery] OpportunityProductFilterDto query)
    {
        var productsQuery = _dbContext.OpportunityProducts.AsQueryable();

        if (query.OpportunityId.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            productsQuery = productsQuery.Where(x =>
                x.ProductName.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.Opportunity.Topic.ToLower().Contains(search));
        }

        productsQuery = productsQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            productsQuery = productsQuery.OrderBy(x => x.SortOrder).ThenBy(x => x.ProductName);
        }

        return Ok(await ProjectProducts(productsQuery).ToPagedAsync(query));
    }

    [HttpGet("api/opportunity-products/{id:guid}")]
    [HasPermission("OpportunityProducts.View")]
    public async Task<ActionResult<OpportunityProductDto>> GetProduct(Guid id)
    {
        var product = await ProjectProducts(_dbContext.OpportunityProducts.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("api/opportunities/{opportunityId:guid}/products")]
    [HasPermission("OpportunityProducts.Create")]
    public async Task<ActionResult<OpportunityProductDto>> CreateProduct(Guid opportunityId, UpsertOpportunityProductRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == opportunityId);
        if (opportunity is null)
        {
            return NotFound();
        }

        var resolved = await ResolveRequestDefaultsAsync(opportunity, dto);
        if (resolved.Error is not null)
        {
            return BadRequest(resolved.Error);
        }

        var validationError = ValidateProduct(resolved.Request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var product = new OpportunityProduct { OpportunityId = opportunityId };
        ApplyProductValues(product, resolved.Request);

        _dbContext.OpportunityProducts.Add(product);
        await _dbContext.SaveChangesAsync();
        await RecalculateOpportunityRevenueAsync(opportunityId);

        var created = await ProjectProducts(_dbContext.OpportunityProducts.Where(x => x.Id == product.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Opportunity product was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("api/opportunity-products/{id:guid}")]
    [HasPermission("OpportunityProducts.Update")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpsertOpportunityProductRequestDto dto)
    {
        var product = await _dbContext.OpportunityProducts.FirstOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == product.OpportunityId);
        if (opportunity is null)
        {
            return NotFound();
        }

        var resolved = await ResolveRequestDefaultsAsync(opportunity, dto);
        if (resolved.Error is not null)
        {
            return BadRequest(resolved.Error);
        }

        var validationError = ValidateProduct(resolved.Request);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyProductValues(product, resolved.Request);
        await _dbContext.SaveChangesAsync();
        await RecalculateOpportunityRevenueAsync(product.OpportunityId);

        return NoContent();
    }

    [HttpDelete("api/opportunity-products/{id:guid}")]
    [HasPermission("OpportunityProducts.Delete")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _dbContext.OpportunityProducts.FirstOrDefaultAsync(x => x.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        var opportunityId = product.OpportunityId;
        product.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await RecalculateOpportunityRevenueAsync(opportunityId);

        return NoContent();
    }

    private async Task RecalculateOpportunityRevenueAsync(Guid opportunityId)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == opportunityId);
        if (opportunity is null)
        {
            return;
        }

        var lineTotal = await _dbContext.OpportunityProducts
            .Where(x => x.OpportunityId == opportunityId)
            .SumAsync(x => x.LineTotal);

        opportunity.EstimatedRevenue = lineTotal;
        opportunity.WeightedRevenue = Math.Round(lineTotal * opportunity.Probability / 100m, 2);
        await _dbContext.SaveChangesAsync();
    }

    private static string? ValidateProduct(UpsertOpportunityProductRequestDto dto)
    {
        if (dto.ProductId is null && string.IsNullOrWhiteSpace(dto.ProductName))
        {
            return "Product name is required when ProductId is not provided.";
        }

        if (dto.Quantity <= 0)
        {
            return "Quantity must be greater than zero.";
        }

        if (dto.UnitPrice < 0)
        {
            return "Unit price cannot be negative.";
        }

        if (dto.DiscountPercent is < 0 or > 100)
        {
            return "Discount percent must be between 0 and 100.";
        }

        if (dto.DiscountAmount is < 0 || dto.TaxAmount is < 0)
        {
            return "Discount and tax amounts cannot be negative.";
        }

        return null;
    }

    private async Task<(UpsertOpportunityProductRequestDto Request, string? Error)> ResolveRequestDefaultsAsync(Opportunity opportunity, UpsertOpportunityProductRequestDto dto)
    {
        Product? product = null;
        if (dto.ProductId.HasValue)
        {
            product = await _dbContext.Products
                .Where(x => x.Id == dto.ProductId.Value)
                .Select(x => new Product
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    StandardPrice = x.StandardPrice,
                    TaxRate = x.TaxRate,
                })
                .FirstOrDefaultAsync();

            if (product is null)
            {
                return (dto, "Product is invalid.");
            }
        }

        var resolved = new UpsertOpportunityProductRequestDto
        {
            ProductId = dto.ProductId,
            ProductName = !string.IsNullOrWhiteSpace(dto.ProductName)
                ? dto.ProductName
                : (product?.Name ?? string.Empty),
            Description = !string.IsNullOrWhiteSpace(dto.Description)
                ? dto.Description
                : product?.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            SortOrder = dto.SortOrder,
        };

        if (resolved.ProductId.HasValue && resolved.UnitPrice <= 0)
        {
            var pricedUnit = await ResolveUnitPriceAsync(opportunity, resolved.ProductId.Value, resolved.Quantity);
            if (pricedUnit.HasValue)
            {
                resolved.UnitPrice = pricedUnit.Value;
            }
            else if (product?.StandardPrice.HasValue == true)
            {
                resolved.UnitPrice = product.StandardPrice.Value;
            }
        }

        if (!resolved.TaxAmount.HasValue && product?.TaxRate.HasValue == true)
        {
            var gross = resolved.Quantity * resolved.UnitPrice;
            var discountAmount = resolved.DiscountAmount ?? (resolved.DiscountPercent.HasValue ? gross * resolved.DiscountPercent.Value / 100m : 0m);
            var taxable = Math.Max(0m, gross - discountAmount);
            resolved.TaxAmount = Math.Round(taxable * product.TaxRate.Value / 100m, 2);
        }

        return (resolved, null);
    }

    private async Task<decimal?> ResolveUnitPriceAsync(Opportunity opportunity, Guid productId, decimal quantity)
    {
        var now = DateTime.UtcNow;
        var normalizedQuantity = quantity <= 0 ? 1 : quantity;

        var defaultPriceListId = await _dbContext.PriceLists
            .Where(x =>
                x.IsActive &&
                x.IsDefault &&
                (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value <= now) &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= now) &&
                (opportunity.CurrencyId.HasValue
                    ? x.CurrencyId == opportunity.CurrencyId.Value
                    : true))
            .OrderByDescending(x => opportunity.CurrencyId.HasValue && x.CurrencyId == opportunity.CurrencyId.Value)
            .ThenBy(x => x.Name)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (defaultPriceListId == Guid.Empty)
        {
            return null;
        }

        var unitPrice = await _dbContext.PriceListItems
            .Where(x =>
                x.PriceListId == defaultPriceListId &&
                x.ProductId == productId &&
                (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value <= now) &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= now) &&
                (!x.MinimumQuantity.HasValue || x.MinimumQuantity.Value <= normalizedQuantity) &&
                (!x.MaximumQuantity.HasValue || x.MaximumQuantity.Value >= normalizedQuantity))
            .OrderByDescending(x => x.MinimumQuantity ?? 0)
            .ThenBy(x => x.CreatedAt)
            .Select(x => x.UnitPrice)
            .FirstOrDefaultAsync();

        return unitPrice == 0m ? null : unitPrice;
    }

    private static void ApplyProductValues(OpportunityProduct product, UpsertOpportunityProductRequestDto dto)
    {
        product.ProductId = dto.ProductId;
        product.ProductName = dto.ProductName.Trim();
        product.Description = TrimToNull(dto.Description);
        product.Quantity = dto.Quantity;
        product.UnitPrice = dto.UnitPrice;
        product.DiscountPercent = dto.DiscountPercent;
        product.TaxAmount = dto.TaxAmount ?? 0m;
        product.SortOrder = dto.SortOrder;

        var gross = dto.Quantity * dto.UnitPrice;
        var discountAmount = dto.DiscountAmount ?? (dto.DiscountPercent.HasValue ? gross * dto.DiscountPercent.Value / 100m : 0m);
        product.DiscountAmount = discountAmount;
        product.LineTotal = Math.Round(gross - discountAmount + product.TaxAmount.Value, 2);
    }

    private static IQueryable<OpportunityProductDto> ProjectProducts(IQueryable<OpportunityProduct> query)
    {
        return query.Select(x => new OpportunityProductDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Description = x.Description,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            DiscountPercent = x.DiscountPercent,
            DiscountAmount = x.DiscountAmount,
            TaxAmount = x.TaxAmount,
            LineTotal = x.LineTotal,
            SortOrder = x.SortOrder
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
