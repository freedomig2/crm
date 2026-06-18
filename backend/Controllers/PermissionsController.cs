using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;

    public PermissionsController(AppDbContext dbContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("Roles.View")]
    public async Task<ActionResult<PagedResult<PermissionDto>>> GetPermissions([FromQuery] ListQueryDto query)
    {
        var permissionsQuery = _dbContext.Permissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            permissionsQuery = permissionsQuery.Where(x => x.Name.ToLower().Contains(search) || x.Module.ToLower().Contains(search) || x.Action.ToLower().Contains(search));
        }

        permissionsQuery = permissionsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = permissionsQuery
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Action)
            .Select(x => new PermissionDto
            {
                Id = x.Id,
                Name = x.Name,
                Module = x.Module,
                Action = x.Action
            });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Roles.View")]
    public async Task<ActionResult<PermissionDto>> GetPermission(Guid id)
    {
        var item = await _dbContext.Permissions
            .Where(x => x.Id == id)
            .Select(x => new PermissionDto
            {
                Id = x.Id,
                Name = x.Name,
                Module = x.Module,
                Action = x.Action
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Roles.Create")]
    public async Task<ActionResult<PermissionDto>> CreatePermission(UpsertPermissionRequestDto dto)
    {
        var name = $"{dto.Module}.{dto.Action}";
        if (await _dbContext.Permissions.AnyAsync(x => x.Name == name))
        {
            return Conflict(new { message = "Permission already exists." });
        }

        var entity = new Permission
        {
            Name = name,
            Module = dto.Module,
            Action = dto.Action
        };

        _dbContext.Permissions.Add(entity);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Permissions", entity.Id.ToString(), "Create", newValues: name);

        return Ok(new PermissionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Module = entity.Module,
            Action = entity.Action
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Roles.Update")]
    public async Task<IActionResult> UpdatePermission(Guid id, UpsertPermissionRequestDto dto)
    {
        var entity = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        var old = entity.Name;
        entity.Module = dto.Module;
        entity.Action = dto.Action;
        entity.Name = $"{dto.Module}.{dto.Action}";

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Permissions", id.ToString(), "Update", old, entity.Name);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Roles.Delete")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var entity = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return NotFound();
        }

        entity.IsDeleted = true;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Permissions", id.ToString(), "Delete");
        return NoContent();
    }
}
