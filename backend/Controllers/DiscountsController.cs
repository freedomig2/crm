using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/discounts")]
public class DiscountsController : ControllerBase
{
    private const string DiscountTypeCategoryCode = "DISCOUNT_TYPE";
    private readonly AppDbContext _dbContext;

    public DiscountsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("Discounts.View")]
    public async Task<ActionResult<PagedResult<DiscountDto>>> GetDiscounts([FromQuery] DiscountFilterDto query)
    {
        var discounts = _dbContext.Discounts.AsQueryable();

        if (query.DiscountTypeId.HasValue)
        {
            discounts = discounts.Where(x => x.DiscountTypeId == query.DiscountTypeId.Value);
        }

        if (query.IsActive.HasValue)
        {
            discounts = discounts.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            discounts = discounts.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        discounts = discounts.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            discounts = discounts.OrderBy(x => x.Name);
        }

        return Ok(await ProjectDiscounts(discounts).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Discounts.View")]
    public async Task<ActionResult<DiscountDto>> GetDiscount(Guid id)
    {
        var item = await ProjectDiscounts(_dbContext.Discounts.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Discounts.Create")]
    public async Task<ActionResult<DiscountDto>> CreateDiscount(UpsertDiscountRequestDto dto)
    {
        var validationError = await ValidateAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new Discount();
        ApplyValues(item, dto);

        _dbContext.Discounts.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectDiscounts(_dbContext.Discounts.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Discount was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Discounts.Update")]
    public async Task<IActionResult> UpdateDiscount(Guid id, UpsertDiscountRequestDto dto)
    {
        var item = await _dbContext.Discounts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = await ValidateAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyValues(item, dto);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Discounts.Delete")]
    public async Task<IActionResult> DeleteDiscount(Guid id)
    {
        var item = await _dbContext.Discounts.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateAsync(Guid? id, UpsertDiscountRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return "Code is required.";
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.Discounts.AnyAsync(x => x.Id != id && x.Code == code))
        {
            return "Code already exists.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.DiscountTypeId && x.LookupCategory.Code == DiscountTypeCategoryCode))
        {
            return "Discount type is invalid.";
        }

        if (dto.Value < 0 || dto.MaximumAmount < 0)
        {
            return "Discount value cannot be negative.";
        }

        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom.Value)
        {
            return "Effective To date must be greater than or equal to Effective From date.";
        }

        return null;
    }

    private static void ApplyValues(Discount item, UpsertDiscountRequestDto dto)
    {
        item.Name = dto.Name.Trim();
        item.Code = dto.Code.Trim().ToUpperInvariant();
        item.DiscountTypeId = dto.DiscountTypeId;
        item.Value = dto.Value;
        item.MaximumAmount = dto.MaximumAmount;
        item.IsStackable = dto.IsStackable;
        item.IsActive = dto.IsActive;
        item.EffectiveFrom = dto.EffectiveFrom;
        item.EffectiveTo = dto.EffectiveTo;
        item.Description = TrimToNull(dto.Description);
    }

    private static IQueryable<DiscountDto> ProjectDiscounts(IQueryable<Discount> query)
    {
        return query.Select(x => new DiscountDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            DiscountTypeId = x.DiscountTypeId,
            DiscountTypeName = x.DiscountType.Name,
            Value = x.Value,
            MaximumAmount = x.MaximumAmount,
            IsStackable = x.IsStackable,
            IsActive = x.IsActive,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            Description = x.Description,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
