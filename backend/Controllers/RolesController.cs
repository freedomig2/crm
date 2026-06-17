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
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserContext _currentUser;

    public RolesController(AppDbContext dbContext, IAuditService auditService, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [HasPermission("Roles.View")]
    public async Task<ActionResult<PagedResult<RoleDto>>> GetRoles([FromQuery] ListQueryDto query)
    {
        var rolesQuery = _dbContext.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            rolesQuery = rolesQuery.Where(x => (x.Name ?? string.Empty).ToLower().Contains(search) || (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        rolesQuery = rolesQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = rolesQuery.Select(x => new RoleDto
        {
            Id = x.Id,
            Name = x.Name ?? string.Empty,
            Description = x.Description
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Roles.View")]
    public async Task<ActionResult<RoleDto>> GetRole(Guid id)
    {
        var role = await _dbContext.Roles
            .Where(x => x.Id == id)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty,
                Description = x.Description
            })
            .FirstOrDefaultAsync();

        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    [HasPermission("Roles.Create")]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleRequestDto dto)
    {
        if (await _dbContext.Roles.AnyAsync(x => x.Name == dto.Name && !x.IsDeleted))
        {
            return Conflict(new { message = "Role already exists." });
        }

        var role = new AppRole
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToUpperInvariant(),
            Description = dto.Description,
            CreatedBy = _currentUser.UserEmail
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("Roles", role.Id.ToString(), "Create", newValues: role.Name);
        return Ok(new RoleDto { Id = role.Id, Name = role.Name ?? string.Empty, Description = role.Description });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Roles.Update")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleRequestDto dto)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (role is null)
        {
            return NotFound();
        }

        var old = role.Name;
        role.Name = dto.Name;
        role.NormalizedName = dto.Name.ToUpperInvariant();
        role.Description = dto.Description;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = _currentUser.UserEmail;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Roles", role.Id.ToString(), "Update", old, role.Name);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Roles.Delete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (role is null)
        {
            return NotFound();
        }

        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = _currentUser.UserEmail;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Roles", id.ToString(), "Delete");
        return NoContent();
    }

    [HttpPost("{id:guid}/permissions")]
    [HasPermission("Roles.Update")]
    public async Task<IActionResult> AssignPermissions(Guid id, IdsRequestDto dto)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (role is null)
        {
            return NotFound();
        }

        var existing = _dbContext.RolePermissions.Where(x => x.RoleId == id);
        _dbContext.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in dto.Ids.Distinct())
        {
            _dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = id,
                PermissionId = permissionId,
                CreatedBy = _currentUser.UserEmail
            });
        }

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Roles", id.ToString(), "AssignPermissions", newValues: string.Join(',', dto.Ids));
        return NoContent();
    }
}
