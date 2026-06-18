using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;

    public UsersController(UserManager<AppUser> userManager, AppDbContext dbContext, IAuditService auditService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("Users.View")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] ListQueryDto query)
    {
        var usersQuery = _userManager.Users
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(x => (x.Email ?? "").ToLower().Contains(search) || (x.FirstName ?? "").ToLower().Contains(search) || (x.LastName ?? "").ToLower().Contains(search));
        }

        usersQuery = usersQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var users = await usersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var total = await usersQuery.CountAsync();
        var dtos = new List<UserDto>();

        foreach (var user in users)
        {
            dtos.Add(await MapUserAsync(user));
        }

        return Ok(new PagedResult<UserDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Users.View")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return user is null ? NotFound() : Ok(await MapUserAsync(user));
    }

    [HttpPost]
    [HasPermission("Users.Create")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequestDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
        {
            return Conflict(new { message = "User already exists." });
        }

        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsEnabled = dto.IsEnabled,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _auditService.LogAsync("Users", user.Id.ToString(), "Create", newValues: $"{{\"Email\":\"{user.Email}\"}}");
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, await MapUserAsync(user));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Users.Update")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequestDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null)
        {
            return NotFound();
        }

        var old = $"{{\"FirstName\":\"{user.FirstName}\",\"LastName\":\"{user.LastName}\",\"IsEnabled\":{user.IsEnabled.ToString().ToLowerInvariant()}}}";
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsEnabled = dto.IsEnabled;
        await _userManager.UpdateAsync(user);

        var @new = $"{{\"FirstName\":\"{user.FirstName}\",\"LastName\":\"{user.LastName}\",\"IsEnabled\":{user.IsEnabled.ToString().ToLowerInvariant()}}}";
        await _auditService.LogAsync("Users", user.Id.ToString(), "Update", old, @new);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Users.Delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null)
        {
            return NotFound();
        }

        user.IsDeleted = true;
        user.IsEnabled = false;

        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync("Users", user.Id.ToString(), "Delete");

        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    [HasPermission("Users.Update")]
    public async Task<IActionResult> AssignRoles(Guid id, IdsRequestDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _dbContext.Roles.Where(x => dto.Ids.Contains(x.Id) && !x.IsDeleted).ToListAsync();
        var current = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, current);
        await _userManager.AddToRolesAsync(user, roles.Select(x => x.Name!).ToArray());

        await _auditService.LogAsync("Users", user.Id.ToString(), "AssignRoles", newValues: string.Join(',', roles.Select(x => x.Name)));
        return NoContent();
    }

    [HttpPost("{id:guid}/teams")]
    [HasPermission("Users.Update")]
    public async Task<IActionResult> AssignTeams(Guid id, IdsRequestDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null)
        {
            return NotFound();
        }

        var existing = _dbContext.UserTeams.Where(x => x.UserId == id);
        _dbContext.UserTeams.RemoveRange(existing);
        foreach (var teamId in dto.Ids.Distinct())
        {
            _dbContext.UserTeams.Add(new UserTeam
            {
                UserId = id,
                TeamId = teamId
            });
        }

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Users", id.ToString(), "AssignTeams", newValues: string.Join(',', dto.Ids));
        return NoContent();
    }

    [HttpPost("{id:guid}/department")]
    [HasPermission("Users.Update")]
    public async Task<IActionResult> AssignDepartment(Guid id, IdsRequestDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (user is null)
        {
            return NotFound();
        }

        var existing = _dbContext.UserDepartments.Where(x => x.UserId == id);
        _dbContext.UserDepartments.RemoveRange(existing);
        foreach (var departmentId in dto.Ids.Distinct())
        {
            _dbContext.UserDepartments.Add(new UserDepartment
            {
                UserId = id,
                DepartmentId = departmentId
            });
        }

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Users", id.ToString(), "AssignDepartment", newValues: string.Join(',', dto.Ids));
        return NoContent();
    }

    private async Task<UserDto> MapUserAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _dbContext.RolePermissions
            .Where(x => roles.Contains(x.Role.Name!))
            .Select(x => x.Permission.Name)
            .Distinct()
            .ToListAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEnabled = user.IsEnabled,
            IsLocked = await _userManager.IsLockedOutAsync(user),
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,
            Roles = roles.ToList(),
            Permissions = permissions
        };
    }
}
