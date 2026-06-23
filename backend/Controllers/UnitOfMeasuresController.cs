using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/unit-of-measures")]
public class UnitOfMeasuresController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public UnitOfMeasuresController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("UnitOfMeasures.View")]
    public async Task<ActionResult<PagedResult<UnitOfMeasureDto>>> GetUnitOfMeasures([FromQuery] UnitOfMeasureFilterDto query)
    {
        var items = _dbContext.UnitOfMeasures.AsQueryable();

        if (query.IsActive.HasValue)
        {
            items = items.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            items = items.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search));
        }

        items = items.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            items = items.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        }

        return Ok(await ProjectItems(items).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("UnitOfMeasures.View")]
    public async Task<ActionResult<UnitOfMeasureDto>> GetUnitOfMeasure(Guid id)
    {
        var item = await ProjectItems(_dbContext.UnitOfMeasures.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("UnitOfMeasures.Create")]
    public async Task<ActionResult<UnitOfMeasureDto>> CreateUnitOfMeasure(UpsertUnitOfMeasureRequestDto dto)
    {
        var validationError = await ValidateAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new UnitOfMeasure();
        ApplyValues(item, dto);

        if (item.IsDefault)
        {
            await DisableOtherDefaultsAsync(null);
        }

        _dbContext.UnitOfMeasures.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectItems(_dbContext.UnitOfMeasures.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Unit of measure was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("UnitOfMeasures.Update")]
    public async Task<IActionResult> UpdateUnitOfMeasure(Guid id, UpsertUnitOfMeasureRequestDto dto)
    {
        var item = await _dbContext.UnitOfMeasures.FirstOrDefaultAsync(x => x.Id == id);
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
        if (item.IsDefault)
        {
            await DisableOtherDefaultsAsync(id);
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("UnitOfMeasures.Delete")]
    public async Task<IActionResult> DeleteUnitOfMeasure(Guid id)
    {
        var item = await _dbContext.UnitOfMeasures.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (await _dbContext.Products.AnyAsync(x => x.UnitOfMeasureId == id))
        {
            return BadRequest("Cannot delete a unit of measure that is used by products.");
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateAsync(Guid? id, UpsertUnitOfMeasureRequestDto dto)
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
        if (await _dbContext.UnitOfMeasures.AnyAsync(x => x.Id != id && x.Code == code))
        {
            return "Code already exists.";
        }

        if (await _dbContext.UnitOfMeasures.AnyAsync(x => x.Id != id && x.Name == dto.Name.Trim()))
        {
            return "Name already exists.";
        }

        return null;
    }

    private async Task DisableOtherDefaultsAsync(Guid? currentId)
    {
        var defaults = await _dbContext.UnitOfMeasures
            .Where(x => x.IsDefault && (!currentId.HasValue || x.Id != currentId.Value))
            .ToListAsync();

        foreach (var item in defaults)
        {
            item.IsDefault = false;
        }
    }

    private static void ApplyValues(UnitOfMeasure item, UpsertUnitOfMeasureRequestDto dto)
    {
        item.Name = dto.Name.Trim();
        item.Code = dto.Code.Trim().ToUpperInvariant();
        item.Description = TrimToNull(dto.Description);
        item.SortOrder = dto.SortOrder;
        item.IsDefault = dto.IsDefault;
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<UnitOfMeasureDto> ProjectItems(IQueryable<UnitOfMeasure> query)
    {
        return query.Select(x => new UnitOfMeasureDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Description = x.Description,
            SortOrder = x.SortOrder,
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
