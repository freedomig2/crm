using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/lookup-categories")]
public class LookupCategoriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserContext _currentUser;

    public LookupCategoriesController(AppDbContext dbContext, IAuditService auditService, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [HasPermission("ReferenceData.View")]
    public async Task<ActionResult<PagedResult<LookupCategoryDto>>> GetCategories([FromQuery] ListQueryDto query)
    {
        var categoriesQuery = _dbContext.LookupCategories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            categoriesQuery = categoriesQuery.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
        }

        categoriesQuery = categoriesQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = categoriesQuery.Select(x => new LookupCategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Description = x.Description,
            IsActive = x.IsActive
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("ReferenceData.View")]
    public async Task<ActionResult<LookupCategoryDto>> GetCategory(Guid id)
    {
        var item = await _dbContext.LookupCategories
            .Where(x => x.Id == id)
            .Select(x => new LookupCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("ReferenceData.Create")]
    public async Task<ActionResult<LookupCategoryDto>> CreateCategory(UpsertLookupCategoryRequestDto dto)
    {
        var item = new LookupCategory
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedBy = _currentUser.UserEmail
        };

        _dbContext.LookupCategories.Add(item);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("LookupCategory", item.Id.ToString(), "Create", newValues: item.Code);
        return Ok(new LookupCategoryDto { Id = item.Id, Name = item.Name, Code = item.Code, Description = item.Description, IsActive = item.IsActive });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("ReferenceData.Update")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpsertLookupCategoryRequestDto dto)
    {
        var item = await _dbContext.LookupCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Name = dto.Name;
        item.Code = dto.Code;
        item.Description = dto.Description;
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = _currentUser.UserEmail;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("LookupCategory", id.ToString(), "Update", newValues: item.Code);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("ReferenceData.Delete")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var item = await _dbContext.LookupCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = _currentUser.UserEmail;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("LookupCategory", id.ToString(), "Delete");
        return NoContent();
    }
}
