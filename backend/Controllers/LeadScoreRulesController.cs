using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/lead-score-rules")]
public class LeadScoreRulesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILeadScoringService _leadScoringService;

    public LeadScoreRulesController(AppDbContext dbContext, ILeadScoringService leadScoringService)
    {
        _dbContext = dbContext;
        _leadScoringService = leadScoringService;
    }

    [HttpGet]
    [HasPermission("LeadScoreRules.View")]
    public async Task<ActionResult<PagedResult<LeadScoreRuleDto>>> GetRules([FromQuery] LeadScoreRuleFilterDto query)
    {
        var rulesQuery = _dbContext.LeadScoreRules.AsQueryable();

        if (query.RuleTypeId.HasValue)
        {
            rulesQuery = rulesQuery.Where(x => x.RuleTypeId == query.RuleTypeId.Value);
        }

        if (query.IsActive.HasValue)
        {
            rulesQuery = rulesQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            rulesQuery = rulesQuery.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.RuleType.Name.ToLower().Contains(search));
        }

        rulesQuery = rulesQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            rulesQuery = rulesQuery.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        }

        return Ok(await ProjectRules(rulesQuery).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("LeadScoreRules.View")]
    public async Task<ActionResult<LeadScoreRuleDto>> GetRule(Guid id)
    {
        var item = await ProjectRules(_dbContext.LeadScoreRules.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("LeadScoreRules.Create")]
    public async Task<ActionResult<LeadScoreRuleDto>> CreateRule(UpsertLeadScoreRuleRequestDto dto)
    {
        if (await _dbContext.LeadScoreRules.AnyAsync(x => x.Code == dto.Code.Trim()))
        {
            return BadRequest("Lead score rule code already exists.");
        }

        var validationError = await ValidateRuleReferencesAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new LeadScoreRule();
        ApplyRuleValues(item, dto);
        _dbContext.LeadScoreRules.Add(item);
        await _dbContext.SaveChangesAsync();
        await _leadScoringService.RecalculateAllLeadScoresAsync();

        var created = await ProjectRules(_dbContext.LeadScoreRules.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Lead score rule was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("LeadScoreRules.Update")]
    public async Task<IActionResult> UpdateRule(Guid id, UpsertLeadScoreRuleRequestDto dto)
    {
        var item = await _dbContext.LeadScoreRules.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (await _dbContext.LeadScoreRules.AnyAsync(x => x.Id != id && x.Code == dto.Code.Trim()))
        {
            return BadRequest("Lead score rule code already exists.");
        }

        var validationError = await ValidateRuleReferencesAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyRuleValues(item, dto);
        await _dbContext.SaveChangesAsync();
        await _leadScoringService.RecalculateAllLeadScoresAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("LeadScoreRules.Delete")]
    public async Task<IActionResult> DeleteRule(Guid id)
    {
        var item = await _dbContext.LeadScoreRules.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        await _leadScoringService.RecalculateAllLeadScoresAsync();
        return NoContent();
    }

    [HttpPost("run")]
    [HasPermission("LeadScoreRules.Run")]
    public async Task<ActionResult<LeadScoreRuleRunResultDto>> RunRules()
    {
        var count = await _leadScoringService.RecalculateAllLeadScoresAsync();
        return Ok(new LeadScoreRuleRunResultDto { RecalculatedLeads = count });
    }

    private async Task<string?> ValidateRuleReferencesAsync(UpsertLeadScoreRuleRequestDto dto)
    {
        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.RuleTypeId && x.LookupCategory.Code == "LEAD_SCORE_RULE_TYPE"))
        {
            return "Rule type is required.";
        }

        if (dto.ScoreValue == 0)
        {
            return "Score value must not be zero.";
        }

        return null;
    }

    private static void ApplyRuleValues(LeadScoreRule item, UpsertLeadScoreRuleRequestDto dto)
    {
        item.Name = dto.Name.Trim();
        item.Code = dto.Code.Trim();
        item.Description = TrimToNull(dto.Description);
        item.RuleTypeId = dto.RuleTypeId;
        item.FieldName = TrimToNull(dto.FieldName);
        item.Operator = TrimToNull(dto.Operator);
        item.CompareValue = TrimToNull(dto.CompareValue);
        item.ScoreValue = dto.ScoreValue;
        item.SortOrder = dto.SortOrder;
        item.IsActive = dto.IsActive;
    }

    private static IQueryable<LeadScoreRuleDto> ProjectRules(IQueryable<LeadScoreRule> query)
    {
        return query.Select(x => new LeadScoreRuleDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Description = x.Description,
            RuleTypeId = x.RuleTypeId,
            RuleTypeName = x.RuleType.Name,
            FieldName = x.FieldName,
            Operator = x.Operator,
            CompareValue = x.CompareValue,
            ScoreValue = x.ScoreValue,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
