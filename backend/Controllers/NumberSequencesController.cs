using System.Text.Json;
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
[Route("api/number-sequences")]
public class NumberSequencesController : ControllerBase
{
    private const string ResetFrequencyCategory = "NUMBER_SEQUENCE_RESET_FREQUENCY";
    private static readonly Regex SequenceCodePattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAuditService _auditService;

    public NumberSequencesController(
        AppDbContext dbContext,
        INumberSequenceService numberSequenceService,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("NumberSequences.View")]
    public async Task<ActionResult<PagedResult<NumberSequenceDto>>> GetNumberSequences([FromQuery] NumberSequenceFilterDto query)
    {
        var sequences = _dbContext.NumberSequences.AsQueryable();

        if (query.ResetFrequencyId.HasValue)
        {
            sequences = sequences.Where(x => x.ResetFrequencyId == query.ResetFrequencyId.Value);
        }

        if (query.IsActive.HasValue)
        {
            sequences = sequences.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            sequences = sequences.Where(x =>
                x.EntityName.ToLower().Contains(search) ||
                x.SequenceCode.ToLower().Contains(search) ||
                x.Prefix.ToLower().Contains(search) ||
                (x.FormatPreview ?? string.Empty).ToLower().Contains(search) ||
                (x.ResetFrequency != null ? x.ResetFrequency.Name : string.Empty).ToLower().Contains(search));
        }

        sequences = sequences.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            sequences = sequences.OrderBy(x => x.EntityName);
        }

        return Ok(await ProjectSequences(sequences).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("NumberSequences.View")]
    public async Task<ActionResult<NumberSequenceDto>> GetNumberSequence(Guid id)
    {
        var item = await ProjectSequences(_dbContext.NumberSequences.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("by-code/{sequenceCode}")]
    [HasPermission("NumberSequences.View")]
    public async Task<ActionResult<NumberSequenceDto>> GetByCode(string sequenceCode)
    {
        var normalizedCode = sequenceCode.Trim().ToUpperInvariant();
        var item = await ProjectSequences(_dbContext.NumberSequences.Where(x => x.SequenceCode == normalizedCode)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("NumberSequences.Create")]
    public async Task<ActionResult<NumberSequenceDto>> CreateNumberSequence(UpsertNumberSequenceRequestDto dto)
    {
        var validationError = await ValidateAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        if (await _dbContext.NumberSequences.AnyAsync(x => x.SequenceCode == dto.SequenceCode.Trim()))
        {
            return BadRequest("Sequence code already exists.");
        }

        var item = new NumberSequence();
        ApplyValues(item, dto);
        NumberSequenceFormatter.RefreshFormatPreview(item, DateTime.UtcNow);

        _dbContext.NumberSequences.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectSequences(_dbContext.NumberSequences.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Number sequence was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("NumberSequences.Update")]
    public async Task<IActionResult> UpdateNumberSequence(Guid id, UpsertNumberSequenceRequestDto dto)
    {
        var item = await _dbContext.NumberSequences
            .Include(x => x.ResetFrequency)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var validationError = await ValidateAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        if (await _dbContext.NumberSequences.AnyAsync(x => x.Id != id && x.SequenceCode == dto.SequenceCode.Trim()))
        {
            return BadRequest("Sequence code already exists.");
        }

        ApplyValues(item, dto);
        NumberSequenceFormatter.RefreshFormatPreview(item, DateTime.UtcNow);

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("NumberSequences.Delete")]
    public async Task<IActionResult> DeleteNumberSequence(Guid id)
    {
        var item = await _dbContext.NumberSequences.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/preview")]
    [HasPermission("NumberSequences.Preview")]
    public async Task<ActionResult<NumberSequencePreviewDto>> Preview(Guid id)
    {
        try
        {
            var preview = await _numberSequenceService.PreviewAsync(id);
            await _auditService.LogAsync(nameof(NumberSequence), id.ToString(), "Preview", newValues: JsonSerializer.Serialize(new { preview }));
            return Ok(new NumberSequencePreviewDto { Preview = preview });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/reset")]
    [HasPermission("NumberSequences.Reset")]
    public async Task<IActionResult> Reset(Guid id)
    {
        try
        {
            await _numberSequenceService.ResetAsync(id);
            await _auditService.LogAsync(nameof(NumberSequence), id.ToString(), "Reset", newValues: JsonSerializer.Serialize(new { resetAt = DateTime.UtcNow }));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<string?> ValidateAsync(UpsertNumberSequenceRequestDto dto)
    {
        var sequenceCode = dto.SequenceCode.Trim();

        if (string.IsNullOrWhiteSpace(dto.EntityName))
        {
            return "Entity name is required.";
        }

        if (string.IsNullOrWhiteSpace(sequenceCode))
        {
            return "Sequence code is required.";
        }

        if (sequenceCode != sequenceCode.ToUpperInvariant() || sequenceCode.Contains(' ') || !SequenceCodePattern.IsMatch(sequenceCode))
        {
            return "Sequence code must be uppercase with no spaces.";
        }

        if (string.IsNullOrWhiteSpace(dto.Prefix))
        {
            return "Prefix is required.";
        }

        if (dto.MinimumDigits is < 3 or > 12)
        {
            return "Minimum digits must be between 3 and 12.";
        }

        if (dto.CurrentNumber < 0)
        {
            return "Current number cannot be negative.";
        }

        if (dto.NextNumber <= dto.CurrentNumber)
        {
            return "Next number must be greater than current number.";
        }

        var separator = string.IsNullOrWhiteSpace(dto.Separator) ? "-" : dto.Separator.Trim();
        if (separator.Length > 3)
        {
            return "Separator cannot exceed 3 characters.";
        }

        if (dto.ResetFrequencyId.HasValue &&
            !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ResetFrequencyId.Value && x.LookupCategory.Code == ResetFrequencyCategory))
        {
            return "Reset frequency is invalid.";
        }

        return null;
    }

    private static void ApplyValues(NumberSequence item, UpsertNumberSequenceRequestDto dto)
    {
        item.EntityName = dto.EntityName.Trim();
        item.SequenceCode = dto.SequenceCode.Trim();
        item.Prefix = dto.Prefix.Trim();
        item.Suffix = TrimToNull(dto.Suffix);
        item.Separator = string.IsNullOrWhiteSpace(dto.Separator) ? "-" : dto.Separator.Trim();
        item.CurrentNumber = dto.CurrentNumber;
        item.NextNumber = dto.NextNumber;
        item.MinimumDigits = dto.MinimumDigits;
        item.ResetFrequencyId = dto.ResetFrequencyId;
        item.LastResetDate = dto.LastResetDate;
        item.IncludeYear = dto.IncludeYear;
        item.IncludeMonth = dto.IncludeMonth;
        item.IncludeDay = dto.IncludeDay;
        item.Description = TrimToNull(dto.Description);
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<NumberSequenceDto> ProjectSequences(IQueryable<NumberSequence> query)
    {
        return query.Select(x => new NumberSequenceDto
        {
            Id = x.Id,
            EntityName = x.EntityName,
            SequenceCode = x.SequenceCode,
            Prefix = x.Prefix,
            Suffix = x.Suffix,
            Separator = x.Separator,
            CurrentNumber = x.CurrentNumber,
            NextNumber = x.NextNumber,
            MinimumDigits = x.MinimumDigits,
            ResetFrequencyId = x.ResetFrequencyId,
            ResetFrequencyName = x.ResetFrequency != null ? x.ResetFrequency.Name : null,
            LastResetDate = x.LastResetDate,
            IncludeYear = x.IncludeYear,
            IncludeMonth = x.IncludeMonth,
            IncludeDay = x.IncludeDay,
            FormatPreview = x.FormatPreview,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
