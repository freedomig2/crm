using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class PriceListItemsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PriceListItemsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("api/price-lists/{priceListId:guid}/items")]
    [HasPermission("PriceLists.View")]
    public async Task<ActionResult<PagedResult<PriceListItemDto>>> GetPriceListItems(Guid priceListId, [FromQuery] PriceListItemFilterDto query)
    {
        if (!await _dbContext.PriceLists.AnyAsync(x => x.Id == priceListId))
        {
            return NotFound();
        }

        query.PriceListId = priceListId;
        var items = _dbContext.PriceListItems.AsQueryable();

        if (query.PriceListId.HasValue)
        {
            items = items.Where(x => x.PriceListId == query.PriceListId.Value);
        }

        if (query.ProductId.HasValue)
        {
            items = items.Where(x => x.ProductId == query.ProductId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            items = items.Where(x =>
                x.Product.ProductCode.ToLower().Contains(search) ||
                x.Product.Name.ToLower().Contains(search));
        }

        items = items.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            items = items.OrderBy(x => x.Product.Name);
        }

        return Ok(await ProjectItems(items).ToPagedAsync(query));
    }

    [HttpPost("api/price-lists/{priceListId:guid}/items")]
    [HasPermission("PriceLists.Update")]
    public async Task<ActionResult<PriceListItemDto>> CreatePriceListItem(Guid priceListId, UpsertPriceListItemRequestDto dto)
    {
        if (!await _dbContext.PriceLists.AnyAsync(x => x.Id == priceListId))
        {
            return NotFound();
        }

        var validationError = await ValidateItemAsync(null, priceListId, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new PriceListItem { PriceListId = priceListId };
        ApplyValues(item, dto);

        _dbContext.PriceListItems.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectItems(_dbContext.PriceListItems.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Price list item was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("api/price-list-items/{id:guid}")]
    [HasPermission("PriceLists.Update")]
    public async Task<IActionResult> UpdatePriceListItem(Guid id, UpsertPriceListItemRequestDto dto)
    {
        var item = await _dbContext.PriceListItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = await ValidateItemAsync(id, item.PriceListId, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyValues(item, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/price-list-items/{id:guid}")]
    [HasPermission("PriceLists.Delete")]
    public async Task<IActionResult> DeletePriceListItem(Guid id)
    {
        var item = await _dbContext.PriceListItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateItemAsync(Guid? id, Guid priceListId, UpsertPriceListItemRequestDto dto)
    {
        if (!await _dbContext.Products.AnyAsync(x => x.Id == dto.ProductId))
        {
            return "Product is invalid.";
        }

        if (dto.UnitPrice < 0)
        {
            return "Unit price cannot be negative.";
        }

        if (dto.MinimumQuantity < 0 || dto.MaximumQuantity < 0)
        {
            return "Minimum and maximum quantity cannot be negative.";
        }

        if (dto.MinimumQuantity.HasValue && dto.MaximumQuantity.HasValue && dto.MaximumQuantity.Value < dto.MinimumQuantity.Value)
        {
            return "Maximum quantity must be greater than or equal to minimum quantity.";
        }

        if (dto.DiscountPercent is < 0 or > 100)
        {
            return "Discount percent must be between 0 and 100.";
        }

        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom.Value)
        {
            return "Effective To date must be greater than or equal to Effective From date.";
        }

        var duplicate = await _dbContext.PriceListItems.AnyAsync(x =>
            x.Id != id &&
            x.PriceListId == priceListId &&
            x.ProductId == dto.ProductId &&
            x.MinimumQuantity == dto.MinimumQuantity &&
            x.MaximumQuantity == dto.MaximumQuantity);

        if (duplicate)
        {
            return "A matching product range already exists in this price list.";
        }

        return null;
    }

    private static void ApplyValues(PriceListItem item, UpsertPriceListItemRequestDto dto)
    {
        item.ProductId = dto.ProductId;
        item.UnitPrice = dto.UnitPrice;
        item.MinimumQuantity = dto.MinimumQuantity;
        item.MaximumQuantity = dto.MaximumQuantity;
        item.DiscountPercent = dto.DiscountPercent;
        item.EffectiveFrom = dto.EffectiveFrom;
        item.EffectiveTo = dto.EffectiveTo;
    }

    private static IQueryable<PriceListItemDto> ProjectItems(IQueryable<PriceListItem> query)
    {
        return query.Select(x => new PriceListItemDto
        {
            Id = x.Id,
            PriceListId = x.PriceListId,
            ProductId = x.ProductId,
            ProductCode = x.Product.ProductCode,
            ProductName = x.Product.Name,
            UnitPrice = x.UnitPrice,
            MinimumQuantity = x.MinimumQuantity,
            MaximumQuantity = x.MaximumQuantity,
            DiscountPercent = x.DiscountPercent,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }
}
