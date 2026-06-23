using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/price-lists")]
public class PriceListsController : ControllerBase
{
    private const string CurrencyCategoryCode = "CURRENCY";
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public PriceListsController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("PriceLists.View")]
    public async Task<ActionResult<PagedResult<PriceListDto>>> GetPriceLists([FromQuery] PriceListFilterDto query)
    {
        var priceLists = _dbContext.PriceLists.AsQueryable();

        if (query.CurrencyId.HasValue)
        {
            priceLists = priceLists.Where(x => x.CurrencyId == query.CurrencyId.Value);
        }

        if (query.IsDefault.HasValue)
        {
            priceLists = priceLists.Where(x => x.IsDefault == query.IsDefault.Value);
        }

        if (query.IsActive.HasValue)
        {
            priceLists = priceLists.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            priceLists = priceLists.Where(x =>
                x.PriceListNumber.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.Currency.Name.ToLower().Contains(search));
        }

        priceLists = priceLists.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            priceLists = priceLists.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name);
        }

        return Ok(await ProjectPriceLists(priceLists).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("PriceLists.View")]
    public async Task<ActionResult<PriceListDto>> GetPriceList(Guid id)
    {
        var item = await ProjectPriceLists(_dbContext.PriceLists.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("PriceLists.Create")]
    public async Task<ActionResult<PriceListDto>> CreatePriceList(UpsertPriceListRequestDto dto)
    {
        var validationError = await ValidatePriceListAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new PriceList();
        ApplyValues(item, dto);
        if (string.IsNullOrWhiteSpace(item.PriceListNumber))
        {
            item.PriceListNumber = await _numberSequenceService.GenerateNextAsync("PRICE_LIST");
        }

        if (await _dbContext.PriceLists.AnyAsync(x => x.PriceListNumber == item.PriceListNumber))
        {
            return BadRequest("Price list number already exists.");
        }

        if (item.IsActive && item.IsDefault)
        {
            await DisableOtherDefaultPriceListsAsync(item.CurrencyId, null);
        }

        _dbContext.PriceLists.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectPriceLists(_dbContext.PriceLists.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Price list was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("PriceLists.Update")]
    public async Task<IActionResult> UpdatePriceList(Guid id, UpsertPriceListRequestDto dto)
    {
        var item = await _dbContext.PriceLists.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = await ValidatePriceListAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyValues(item, dto);
        if (string.IsNullOrWhiteSpace(item.PriceListNumber))
        {
            item.PriceListNumber = await _numberSequenceService.GenerateNextAsync("PRICE_LIST");
        }

        if (await _dbContext.PriceLists.AnyAsync(x => x.Id != id && x.PriceListNumber == item.PriceListNumber))
        {
            return BadRequest("Price list number already exists.");
        }

        if (item.IsActive && item.IsDefault)
        {
            await DisableOtherDefaultPriceListsAsync(item.CurrencyId, id);
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("PriceLists.Delete")]
    public async Task<IActionResult> DeletePriceList(Guid id)
    {
        var item = await _dbContext.PriceLists.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidatePriceListAsync(Guid? id, UpsertPriceListRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.CurrencyId && x.LookupCategory.Code == CurrencyCategoryCode))
        {
            return "Currency is invalid.";
        }

        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom.Value)
        {
            return "Effective To date must be greater than or equal to Effective From date.";
        }

        if (dto.IsActive && dto.IsDefault)
        {
            var exists = await _dbContext.PriceLists.AnyAsync(x =>
                x.Id != id &&
                x.CurrencyId == dto.CurrencyId &&
                x.IsActive &&
                x.IsDefault);

            if (exists)
            {
                return "Only one active default price list is allowed per currency.";
            }
        }

        return null;
    }

    private async Task DisableOtherDefaultPriceListsAsync(Guid currencyId, Guid? currentId)
    {
        var existingDefaults = await _dbContext.PriceLists
            .Where(x => x.CurrencyId == currencyId && x.IsDefault && x.IsActive && (!currentId.HasValue || x.Id != currentId.Value))
            .ToListAsync();

        foreach (var existing in existingDefaults)
        {
            existing.IsDefault = false;
        }
    }

    private static void ApplyValues(PriceList item, UpsertPriceListRequestDto dto)
    {
        item.PriceListNumber = TrimToNull(dto.PriceListNumber) ?? string.Empty;
        item.Name = dto.Name.Trim();
        item.Description = TrimToNull(dto.Description);
        item.CurrencyId = dto.CurrencyId;
        item.EffectiveFrom = dto.EffectiveFrom;
        item.EffectiveTo = dto.EffectiveTo;
        item.IsDefault = dto.IsDefault;
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<PriceListDto> ProjectPriceLists(IQueryable<PriceList> query)
    {
        return query.Select(x => new PriceListDto
        {
            Id = x.Id,
            PriceListNumber = x.PriceListNumber,
            Name = x.Name,
            Description = x.Description,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            IsDefault = x.IsDefault,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
