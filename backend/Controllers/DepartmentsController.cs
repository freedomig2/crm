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
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserContext _currentUser;

    public DepartmentsController(AppDbContext dbContext, IAuditService auditService, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [HasPermission("Departments.View")]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> GetDepartments([FromQuery] ListQueryDto query)
    {
        var departmentsQuery = _dbContext.Departments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            departmentsQuery = departmentsQuery.Where(x => x.Name.ToLower().Contains(search) || (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        departmentsQuery = departmentsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = departmentsQuery.Select(x => new DepartmentDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            ParentDepartmentId = x.ParentDepartmentId,
            IsActive = x.IsActive
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Departments.View")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(Guid id)
    {
        var item = await _dbContext.Departments
            .Where(x => x.Id == id)
            .Select(x => new DepartmentDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ParentDepartmentId = x.ParentDepartmentId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Departments.Create")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(UpsertDepartmentRequestDto dto)
    {
        var item = new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentDepartmentId = dto.ParentDepartmentId,
            IsActive = dto.IsActive,
            CreatedBy = _currentUser.UserEmail
        };

        _dbContext.Departments.Add(item);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Departments", item.Id.ToString(), "Create", newValues: item.Name);

        return Ok(new DepartmentDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            ParentDepartmentId = item.ParentDepartmentId,
            IsActive = item.IsActive
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Departments.Update")]
    public async Task<IActionResult> UpdateDepartment(Guid id, UpsertDepartmentRequestDto dto)
    {
        var item = await _dbContext.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.ParentDepartmentId = dto.ParentDepartmentId;
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = _currentUser.UserEmail;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Departments", id.ToString(), "Update", newValues: item.Name);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Departments.Delete")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var item = await _dbContext.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = _currentUser.UserEmail;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Departments", id.ToString(), "Delete");
        return NoContent();
    }
}
