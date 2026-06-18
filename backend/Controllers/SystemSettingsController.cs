using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/system-settings")]
public class SystemSettingsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;

    public SystemSettingsController(AppDbContext dbContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<ActionResult<PagedResult<SystemSettingDto>>> GetSettings([FromQuery] ListQueryDto query)
    {
        var settingsQuery = _dbContext.SystemSettings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            settingsQuery = settingsQuery.Where(x => x.Category.ToLower().Contains(search) || x.Key.ToLower().Contains(search) || x.Value.ToLower().Contains(search));
        }

        settingsQuery = settingsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = settingsQuery.Select(x => new SystemSettingDto
        {
            Id = x.Id,
            Category = x.Category,
            Key = x.Key,
            Value = x.Value,
            DataType = x.DataType,
            Description = x.Description
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Settings.View")]
    public async Task<ActionResult<SystemSettingDto>> GetSetting(Guid id)
    {
        var setting = await _dbContext.SystemSettings
            .Where(x => x.Id == id)
            .Select(x => new SystemSettingDto
            {
                Id = x.Id,
                Category = x.Category,
                Key = x.Key,
                Value = x.Value,
                DataType = x.DataType,
                Description = x.Description
            })
            .FirstOrDefaultAsync();

        return setting is null ? NotFound() : Ok(setting);
    }

    [HttpPost]
    [HasPermission("Settings.Update")]
    public async Task<ActionResult<SystemSettingDto>> CreateSetting(CreateSystemSettingRequestDto dto)
    {
        if (await _dbContext.SystemSettings.AnyAsync(x => x.Category == dto.Category && x.Key == dto.Key))
        {
            return Conflict(new { message = "A setting with this category/key already exists." });
        }

        var setting = new Entities.SystemSetting
        {
            Category = dto.Category,
            Key = dto.Key,
            Value = dto.Value,
            DataType = dto.DataType,
            Description = dto.Description
        };

        _dbContext.SystemSettings.Add(setting);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("SystemSettings", setting.Id.ToString(), "Create", newValues: setting.Value);

        return Ok(new SystemSettingDto
        {
            Id = setting.Id,
            Category = setting.Category,
            Key = setting.Key,
            Value = setting.Value,
            DataType = setting.DataType,
            Description = setting.Description
        });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Settings.Update")]
    public async Task<IActionResult> UpdateSetting(Guid id, UpdateSystemSettingRequestDto dto)
    {
        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(x => x.Id == id);
        if (setting is null)
        {
            return NotFound();
        }

        if (await _dbContext.SystemSettings.AnyAsync(x => x.Id != id && x.Category == dto.Category && x.Key == dto.Key))
        {
            return Conflict(new { message = "A setting with this category/key already exists." });
        }

        var old = $"{{\"Category\":\"{setting.Category}\",\"Key\":\"{setting.Key}\",\"Value\":\"{setting.Value}\"}}";
        setting.Category = dto.Category;
        setting.Key = dto.Key;
        setting.Value = dto.Value;
        setting.DataType = dto.DataType;
        setting.Description = dto.Description;

        await _dbContext.SaveChangesAsync();
        var @new = $"{{\"Category\":\"{setting.Category}\",\"Key\":\"{setting.Key}\",\"Value\":\"{setting.Value}\"}}";
        await _auditService.LogAsync("SystemSettings", id.ToString(), "Update", old, @new);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Settings.Update")]
    public async Task<IActionResult> DeleteSetting(Guid id)
    {
        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(x => x.Id == id);
        if (setting is null)
        {
            return NotFound();
        }

        setting.IsDeleted = true;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("SystemSettings", id.ToString(), "Delete");
        return NoContent();
    }
}
