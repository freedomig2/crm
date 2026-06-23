using System.Text.RegularExpressions;
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
[Route("api/notification-templates")]
public class NotificationTemplatesController : ControllerBase
{
    private const string NotificationChannelCategoryCode = "NOTIFICATION_CHANNEL";
    private static readonly Regex TemplateCodePattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;

    public NotificationTemplatesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("NotificationTemplates.View")]
    public async Task<ActionResult<PagedResult<NotificationTemplateDto>>> GetTemplates([FromQuery] NotificationTemplateFilterDto query)
    {
        var templates = _dbContext.NotificationTemplates.AsQueryable();

        if (query.ChannelId.HasValue)
        {
            templates = templates.Where(x => x.ChannelId == query.ChannelId.Value);
        }

        if (query.IsSystem.HasValue)
        {
            templates = templates.Where(x => x.IsSystem == query.IsSystem.Value);
        }

        if (query.IsActive.HasValue)
        {
            templates = templates.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            templates = templates.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                x.SubjectTemplate.ToLower().Contains(search) ||
                x.Channel.Name.ToLower().Contains(search));
        }

        templates = templates.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            templates = templates.OrderBy(x => x.Name);
        }

        return Ok(await ProjectTemplates(templates).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("NotificationTemplates.View")]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(Guid id)
    {
        var item = await ProjectTemplates(_dbContext.NotificationTemplates.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("NotificationTemplates.Create")]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(UpsertNotificationTemplateRequestDto dto)
    {
        var validationError = await ValidateTemplateAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.NotificationTemplates.AnyAsync(x => x.Code == code))
        {
            return BadRequest("Notification template code already exists.");
        }

        var template = new NotificationTemplate();
        ApplyTemplateValues(template, dto);

        _dbContext.NotificationTemplates.Add(template);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectTemplates(_dbContext.NotificationTemplates.Where(x => x.Id == template.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Notification template was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("NotificationTemplates.Update")]
    public async Task<IActionResult> UpdateTemplate(Guid id, UpsertNotificationTemplateRequestDto dto)
    {
        var template = await _dbContext.NotificationTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (template is null)
        {
            return NotFound();
        }

        var validationError = await ValidateTemplateAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.NotificationTemplates.AnyAsync(x => x.Id != id && x.Code == code))
        {
            return BadRequest("Notification template code already exists.");
        }

        ApplyTemplateValues(template, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("NotificationTemplates.Delete")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var template = await _dbContext.NotificationTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (template is null)
        {
            return NotFound();
        }

        template.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("NotificationTemplates.View")]
    public async Task<ActionResult<NotificationLookupDto>> GetLookup()
    {
        return Ok(new NotificationLookupDto
        {
            Channels = await GetLookupOptionsAsync(NotificationChannelCategoryCode),
            Statuses = Array.Empty<LookupOptionDto>(),
            Priorities = await GetLookupOptionsAsync("PRIORITY"),
        });
    }

    private async Task<string?> ValidateTemplateAsync(UpsertNotificationTemplateRequestDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Template name is required.";
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return "Template code is required.";
        }

        if (code != dto.Code.Trim() || code.Contains(' ') || !TemplateCodePattern.IsMatch(code))
        {
            return "Template code must be uppercase with no spaces.";
        }

        if (string.IsNullOrWhiteSpace(dto.SubjectTemplate))
        {
            return "Subject template is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.BodyTemplate))
        {
            return "Body template is required.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ChannelId && x.LookupCategory.Code == NotificationChannelCategoryCode))
        {
            return "Notification channel is invalid.";
        }

        return null;
    }

    private static void ApplyTemplateValues(NotificationTemplate template, UpsertNotificationTemplateRequestDto dto)
    {
        template.Name = dto.Name.Trim();
        template.Code = dto.Code.Trim().ToUpperInvariant();
        template.SubjectTemplate = dto.SubjectTemplate.Trim();
        template.BodyTemplate = dto.BodyTemplate.Trim();
        template.ChannelId = dto.ChannelId;
        template.IsSystem = dto.IsSystem;
        template.IsActive = dto.IsActive;
    }

    private static IQueryable<NotificationTemplateDto> ProjectTemplates(IQueryable<NotificationTemplate> query)
    {
        return query.Select(x => new NotificationTemplateDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            SubjectTemplate = x.SubjectTemplate,
            BodyTemplate = x.BodyTemplate,
            ChannelId = x.ChannelId,
            ChannelName = x.Channel.Name,
            ChannelCode = x.Channel.Code ?? string.Empty,
            IsSystem = x.IsSystem,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private async Task<IReadOnlyCollection<LookupOptionDto>> GetLookupOptionsAsync(string categoryCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync();
    }
}
