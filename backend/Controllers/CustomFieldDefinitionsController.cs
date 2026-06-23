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
[Route("api/custom-field-definitions")]
public class CustomFieldDefinitionsController : ControllerBase
{
    private const string DataTypeCategoryCode = "CUSTOM_FIELD_DATA_TYPE";
    private static readonly Regex KeyPattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;

    public CustomFieldDefinitionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("CustomFields.View")]
    public async Task<ActionResult<PagedResult<CustomFieldDefinitionDto>>> GetItems([FromQuery] CustomFieldDefinitionFilterDto query)
    {
        var items = _dbContext.CustomFieldDefinitions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var entityName = query.EntityName.Trim().ToUpperInvariant();
            items = items.Where(x => x.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(query.DataTypeCode))
        {
            var dataTypeCode = query.DataTypeCode.Trim().ToUpperInvariant();
            items = items.Where(x => x.DataType.Code == dataTypeCode);
        }

        if (query.IsRequired.HasValue)
        {
            items = items.Where(x => x.IsRequired == query.IsRequired.Value);
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
                x.FieldKey.ToLower().Contains(search) ||
                x.DisplayName.ToLower().Contains(search) ||
                x.DataType.Name.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        items = items.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            items = items.OrderBy(x => x.EntityName).ThenBy(x => x.SortOrder).ThenBy(x => x.DisplayName);
        }

        return Ok(await Project(items).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("CustomFields.View")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> GetItem(Guid id)
    {
        var item = await Project(_dbContext.CustomFieldDefinitions.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("CustomFields.Create")]
    public async Task<ActionResult<CustomFieldDefinitionDto>> CreateItem(UpsertCustomFieldDefinitionRequestDto dto)
    {
        var (validationError, dataTypeId) = await ValidateAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var entityName = dto.EntityName.Trim().ToUpperInvariant();
        var fieldKey = dto.FieldKey.Trim().ToUpperInvariant();

        if (await _dbContext.CustomFieldDefinitions.AnyAsync(x => x.EntityName == entityName && x.FieldKey == fieldKey))
        {
            return BadRequest("A custom field with the same key already exists for this entity.");
        }

        var item = new CustomFieldDefinition();
        ApplyValues(item, dto, dataTypeId!.Value);

        _dbContext.CustomFieldDefinitions.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await Project(_dbContext.CustomFieldDefinitions.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Custom field was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("CustomFields.Update")]
    public async Task<IActionResult> UpdateItem(Guid id, UpsertCustomFieldDefinitionRequestDto dto)
    {
        var item = await _dbContext.CustomFieldDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var (validationError, dataTypeId) = await ValidateAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var entityName = dto.EntityName.Trim().ToUpperInvariant();
        var fieldKey = dto.FieldKey.Trim().ToUpperInvariant();

        if (await _dbContext.CustomFieldDefinitions.AnyAsync(x => x.Id != id && x.EntityName == entityName && x.FieldKey == fieldKey))
        {
            return BadRequest("A custom field with the same key already exists for this entity.");
        }

        ApplyValues(item, dto, dataTypeId!.Value);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("CustomFields.Delete")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _dbContext.CustomFieldDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<(string? ValidationError, Guid? DataTypeId)> ValidateAsync(UpsertCustomFieldDefinitionRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EntityName))
        {
            return ("Entity name is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.FieldKey))
        {
            return ("Field key is required.", null);
        }

        var fieldKey = dto.FieldKey.Trim().ToUpperInvariant();
        if (fieldKey != dto.FieldKey.Trim() || fieldKey.Contains(' ') || !KeyPattern.IsMatch(fieldKey))
        {
            return ("Field key must be uppercase with no spaces.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.DisplayName))
        {
            return ("Display name is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.DataTypeCode))
        {
            return ("Data type code is required.", null);
        }

        var dataTypeCode = dto.DataTypeCode.Trim().ToUpperInvariant();
        var dataTypeId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == DataTypeCategoryCode && x.Code == dataTypeCode && x.IsActive)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!dataTypeId.HasValue)
        {
            return ("Data type code is invalid. Allowed values: TEXT, NUMBER, DATE, BOOLEAN.", null);
        }

        return (null, dataTypeId.Value);
    }

    private static void ApplyValues(CustomFieldDefinition item, UpsertCustomFieldDefinitionRequestDto dto, Guid dataTypeId)
    {
        item.EntityName = dto.EntityName.Trim().ToUpperInvariant();
        item.FieldKey = dto.FieldKey.Trim().ToUpperInvariant();
        item.DisplayName = dto.DisplayName.Trim();
        item.DataTypeId = dataTypeId;
        item.IsRequired = dto.IsRequired;
        item.IsIndexed = dto.IsIndexed;
        item.DefaultValue = string.IsNullOrWhiteSpace(dto.DefaultValue) ? null : dto.DefaultValue.Trim();
        item.OptionsJson = string.IsNullOrWhiteSpace(dto.OptionsJson) ? null : dto.OptionsJson.Trim();
        item.SortOrder = dto.SortOrder;
        item.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<CustomFieldDefinitionDto> Project(IQueryable<CustomFieldDefinition> query)
    {
        return query.Select(x => new CustomFieldDefinitionDto
        {
            Id = x.Id,
            EntityName = x.EntityName,
            FieldKey = x.FieldKey,
            DisplayName = x.DisplayName,
            DataTypeCode = x.DataType.Code ?? string.Empty,
            DataTypeName = x.DataType.Name,
            IsRequired = x.IsRequired,
            IsIndexed = x.IsIndexed,
            DefaultValue = x.DefaultValue,
            OptionsJson = x.OptionsJson,
            SortOrder = x.SortOrder,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }
}
