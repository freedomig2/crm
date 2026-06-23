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
[Route("api/ai-prompt-templates")]
public class AiPromptTemplatesController : ControllerBase
{
    private static readonly Regex UseCasePattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;

    public AiPromptTemplatesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("AITemplates.View")]
    public async Task<ActionResult<PagedResult<AiPromptTemplateDto>>> GetItems([FromQuery] AiPromptTemplateFilterDto query)
    {
        var items = _dbContext.AiPromptTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.UseCaseCode))
        {
            var useCaseCode = query.UseCaseCode.Trim().ToUpperInvariant();
            items = items.Where(x => x.UseCaseCode == useCaseCode);
        }

        if (query.IsSystem.HasValue)
        {
            items = items.Where(x => x.IsSystem == query.IsSystem.Value);
        }

        if (query.IsActive.HasValue)
        {
            items = items.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            items = items.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.UseCaseCode.ToLower().Contains(search) ||
                x.SystemPrompt.ToLower().Contains(search));
        }

        items = items.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            items = items.OrderBy(x => x.UseCaseCode).ThenBy(x => x.Version);
        }

        return Ok(await Project(items).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("AITemplates.View")]
    public async Task<ActionResult<AiPromptTemplateDto>> GetItem(Guid id)
    {
        var item = await Project(_dbContext.AiPromptTemplates.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("AITemplates.Create")]
    public async Task<ActionResult<AiPromptTemplateDto>> CreateItem(UpsertAiPromptTemplateRequestDto dto)
    {
        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var useCaseCode = dto.UseCaseCode.Trim().ToUpperInvariant();
        if (await _dbContext.AiPromptTemplates.AnyAsync(x => x.UseCaseCode == useCaseCode && x.Version == dto.Version))
        {
            return BadRequest("A prompt template with the same use case and version already exists.");
        }

        var item = new AiPromptTemplate();
        ApplyValues(item, dto);

        _dbContext.AiPromptTemplates.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await Project(_dbContext.AiPromptTemplates.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("AI prompt template was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("AITemplates.Update")]
    public async Task<IActionResult> UpdateItem(Guid id, UpsertAiPromptTemplateRequestDto dto)
    {
        var item = await _dbContext.AiPromptTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var useCaseCode = dto.UseCaseCode.Trim().ToUpperInvariant();
        if (await _dbContext.AiPromptTemplates.AnyAsync(x => x.Id != id && x.UseCaseCode == useCaseCode && x.Version == dto.Version))
        {
            return BadRequest("A prompt template with the same use case and version already exists.");
        }

        ApplyValues(item, dto);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("AITemplates.Delete")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _dbContext.AiPromptTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static string? Validate(UpsertAiPromptTemplateRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Template name is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.UseCaseCode))
        {
            return "Use case code is required.";
        }

        var useCaseCode = dto.UseCaseCode.Trim().ToUpperInvariant();
        if (useCaseCode != dto.UseCaseCode.Trim() || useCaseCode.Contains(' ') || !UseCasePattern.IsMatch(useCaseCode))
        {
            return "Use case code must be uppercase with no spaces.";
        }

        if (string.IsNullOrWhiteSpace(dto.SystemPrompt))
        {
            return "System prompt is required.";
        }

        if (dto.Version < 1)
        {
            return "Version must be 1 or greater.";
        }

        return null;
    }

    private static void ApplyValues(AiPromptTemplate item, UpsertAiPromptTemplateRequestDto dto)
    {
        item.Name = dto.Name.Trim();
        item.UseCaseCode = dto.UseCaseCode.Trim().ToUpperInvariant();
        item.SystemPrompt = dto.SystemPrompt.Trim();
        item.IsSystem = dto.IsSystem;
        item.Version = dto.Version;
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<AiPromptTemplateDto> Project(IQueryable<AiPromptTemplate> query)
    {
        return query.Select(x => new AiPromptTemplateDto
        {
            Id = x.Id,
            Name = x.Name,
            UseCaseCode = x.UseCaseCode,
            SystemPrompt = x.SystemPrompt,
            IsSystem = x.IsSystem,
            Version = x.Version,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }
}
