using System.Text.RegularExpressions;
using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/record-status-definitions")]
public class RecordStatusDefinitionsController : ControllerBase
{
    private static readonly Regex StatusCodePattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;

    public RecordStatusDefinitionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("RecordStatuses.View")]
    public async Task<ActionResult<PagedResult<RecordStatusDefinitionDto>>> GetItems([FromQuery] RecordStatusDefinitionFilterDto query)
    {
        var items = _dbContext.RecordStatusDefinitions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var entityName = query.EntityName.Trim().ToUpperInvariant();
            items = items.Where(x => x.EntityName == entityName);
        }

        if (query.IsClosedState.HasValue)
        {
            items = items.Where(x => x.IsClosedState == query.IsClosedState.Value);
        }

        if (query.IsActive.HasValue)
        {
            items = items.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            items = items.Where(x =>
                x.EntityName.ToLower().Contains(search) ||
                x.StatusCode.ToLower().Contains(search) ||
                x.StatusName.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        items = items.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            items = items.OrderBy(x => x.EntityName).ThenBy(x => x.SortOrder).ThenBy(x => x.StatusName);
        }

        return Ok(await Project(items).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("RecordStatuses.View")]
    public async Task<ActionResult<RecordStatusDefinitionDto>> GetItem(Guid id)
    {
        var item = await Project(_dbContext.RecordStatusDefinitions.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("RecordStatuses.Create")]
    public async Task<ActionResult<RecordStatusDefinitionDto>> CreateItem(UpsertRecordStatusDefinitionRequestDto dto)
    {
        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var entityName = dto.EntityName.Trim().ToUpperInvariant();
        var statusCode = dto.StatusCode.Trim().ToUpperInvariant();

        if (await _dbContext.RecordStatusDefinitions.AnyAsync(x => x.EntityName == entityName && x.StatusCode == statusCode))
        {
            return BadRequest("A record status with the same code already exists for this entity.");
        }

        var item = new RecordStatusDefinition();
        ApplyValues(item, dto);

        _dbContext.RecordStatusDefinitions.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await Project(_dbContext.RecordStatusDefinitions.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Record status was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("RecordStatuses.Update")]
    public async Task<IActionResult> UpdateItem(Guid id, UpsertRecordStatusDefinitionRequestDto dto)
    {
        var item = await _dbContext.RecordStatusDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var entityName = dto.EntityName.Trim().ToUpperInvariant();
        var statusCode = dto.StatusCode.Trim().ToUpperInvariant();

        if (await _dbContext.RecordStatusDefinitions.AnyAsync(x => x.Id != id && x.EntityName == entityName && x.StatusCode == statusCode))
        {
            return BadRequest("A record status with the same code already exists for this entity.");
        }

        ApplyValues(item, dto);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("RecordStatuses.Delete")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _dbContext.RecordStatusDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static string? Validate(UpsertRecordStatusDefinitionRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EntityName))
        {
            return "Entity name is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.StatusCode))
        {
            return "Status code is required.";
        }

        var statusCode = dto.StatusCode.Trim().ToUpperInvariant();
        if (statusCode != dto.StatusCode.Trim() || statusCode.Contains(' ') || !StatusCodePattern.IsMatch(statusCode))
        {
            return "Status code must be uppercase with no spaces.";
        }

        if (string.IsNullOrWhiteSpace(dto.StatusName))
        {
            return "Status name is required.";
        }

        return null;
    }

    private static void ApplyValues(RecordStatusDefinition item, UpsertRecordStatusDefinitionRequestDto dto)
    {
        item.EntityName = dto.EntityName.Trim().ToUpperInvariant();
        item.StatusCode = dto.StatusCode.Trim().ToUpperInvariant();
        item.StatusName = dto.StatusName.Trim();
        item.IsDefault = dto.IsDefault;
        item.IsClosedState = dto.IsClosedState;
        item.SortOrder = dto.SortOrder;
        item.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<RecordStatusDefinitionDto> Project(IQueryable<RecordStatusDefinition> query)
    {
        return query.Select(x => new RecordStatusDefinitionDto
        {
            Id = x.Id,
            EntityName = x.EntityName,
            StatusCode = x.StatusCode,
            StatusName = x.StatusName,
            IsDefault = x.IsDefault,
            IsClosedState = x.IsClosedState,
            SortOrder = x.SortOrder,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }
}
