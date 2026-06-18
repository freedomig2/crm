using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/lookup-values")]
public class LookupValuesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;

    public LookupValuesController(AppDbContext dbContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("ReferenceData.View")]
    public async Task<ActionResult<PagedResult<LookupValueDto>>> GetValues([FromQuery] ListQueryDto query, [FromQuery] Guid? categoryId)
    {
        var valuesQuery = _dbContext.LookupValues.AsQueryable();
        if (categoryId.HasValue)
        {
            valuesQuery = valuesQuery.Where(x => x.LookupCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            valuesQuery = valuesQuery.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
        }

        valuesQuery = valuesQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = valuesQuery.Select(x => new LookupValueDto
        {
            Id = x.Id,
            LookupCategoryId = x.LookupCategoryId,
            Name = x.Name,
            Code = x.Code,
            SortOrder = x.SortOrder,
            IsDefault = x.IsDefault,
            IsActive = x.IsActive
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("ReferenceData.View")]
    public async Task<ActionResult<LookupValueDto>> GetValue(Guid id)
    {
        var item = await _dbContext.LookupValues
            .Where(x => x.Id == id)
            .Select(x => new LookupValueDto
            {
                Id = x.Id,
                LookupCategoryId = x.LookupCategoryId,
                Name = x.Name,
                Code = x.Code,
                SortOrder = x.SortOrder,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("ReferenceData.Create")]
    public async Task<ActionResult<LookupValueDto>> CreateValue(UpsertLookupValueRequestDto dto)
    {
        var item = new LookupValue
        {
            LookupCategoryId = dto.LookupCategoryId,
            Name = dto.Name,
            Code = dto.Code,
            SortOrder = dto.SortOrder,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive
        };

        _dbContext.LookupValues.Add(item);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("LookupValue", item.Id.ToString(), "Create", newValues: item.Code);

        return Ok(new LookupValueDto
        {
            Id = item.Id,
            LookupCategoryId = item.LookupCategoryId,
            Name = item.Name,
            Code = item.Code,
            SortOrder = item.SortOrder,
            IsDefault = item.IsDefault,
            IsActive = item.IsActive
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("ReferenceData.Update")]
    public async Task<IActionResult> UpdateValue(Guid id, UpsertLookupValueRequestDto dto)
    {
        var item = await _dbContext.LookupValues.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.LookupCategoryId = dto.LookupCategoryId;
        item.Name = dto.Name;
        item.Code = dto.Code;
        item.SortOrder = dto.SortOrder;
        item.IsDefault = dto.IsDefault;
        item.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("LookupValue", id.ToString(), "Update", newValues: item.Code);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("ReferenceData.Delete")]
    public async Task<IActionResult> DeleteValue(Guid id)
    {
        var item = await _dbContext.LookupValues.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("LookupValue", id.ToString(), "Delete");
        return NoContent();
    }
}
