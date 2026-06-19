using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class OpportunityCompetitorsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OpportunityCompetitorsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("api/opportunities/{opportunityId:guid}/competitors")]
    [HasPermission("OpportunityCompetitors.View")]
    public async Task<ActionResult<PagedResult<OpportunityCompetitorDto>>> GetOpportunityCompetitors(Guid opportunityId, [FromQuery] OpportunityCompetitorFilterDto query)
    {
        if (!await _dbContext.Opportunities.AnyAsync(x => x.Id == opportunityId))
        {
            return NotFound();
        }

        query.OpportunityId = opportunityId;
        return await GetCompetitors(query);
    }

    [HttpGet("api/opportunity-competitors")]
    [HasPermission("OpportunityCompetitors.View")]
    public async Task<ActionResult<PagedResult<OpportunityCompetitorDto>>> GetCompetitors([FromQuery] OpportunityCompetitorFilterDto query)
    {
        var competitorsQuery = _dbContext.OpportunityCompetitors.AsQueryable();

        if (query.OpportunityId.HasValue)
        {
            competitorsQuery = competitorsQuery.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.ThreatLevelId.HasValue)
        {
            competitorsQuery = competitorsQuery.Where(x => x.ThreatLevelId == query.ThreatLevelId.Value);
        }

        if (query.IsPrimaryCompetitor.HasValue)
        {
            competitorsQuery = competitorsQuery.Where(x => x.IsPrimaryCompetitor == query.IsPrimaryCompetitor.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            competitorsQuery = competitorsQuery.Where(x =>
                x.CompetitorName.ToLower().Contains(search) ||
                (x.Strengths ?? string.Empty).ToLower().Contains(search) ||
                (x.Weaknesses ?? string.Empty).ToLower().Contains(search) ||
                x.Opportunity.Topic.ToLower().Contains(search));
        }

        competitorsQuery = competitorsQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            competitorsQuery = competitorsQuery.OrderByDescending(x => x.IsPrimaryCompetitor).ThenBy(x => x.CompetitorName);
        }

        return Ok(await ProjectCompetitors(competitorsQuery).ToPagedAsync(query));
    }

    [HttpGet("api/opportunity-competitors/{id:guid}")]
    [HasPermission("OpportunityCompetitors.View")]
    public async Task<ActionResult<OpportunityCompetitorDto>> GetCompetitor(Guid id)
    {
        var competitor = await ProjectCompetitors(_dbContext.OpportunityCompetitors.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return competitor is null ? NotFound() : Ok(competitor);
    }

    [HttpPost("api/opportunities/{opportunityId:guid}/competitors")]
    [HasPermission("OpportunityCompetitors.Create")]
    public async Task<ActionResult<OpportunityCompetitorDto>> CreateCompetitor(Guid opportunityId, UpsertOpportunityCompetitorRequestDto dto)
    {
        if (!await _dbContext.Opportunities.AnyAsync(x => x.Id == opportunityId))
        {
            return NotFound();
        }

        var validationError = await ValidateCompetitorAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var competitor = new OpportunityCompetitor { OpportunityId = opportunityId };
        ApplyCompetitorValues(competitor, dto);
        _dbContext.OpportunityCompetitors.Add(competitor);

        if (competitor.IsPrimaryCompetitor)
        {
            await ClearPrimaryCompetitorsAsync(opportunityId, competitor.Id);
        }

        await _dbContext.SaveChangesAsync();

        var created = await ProjectCompetitors(_dbContext.OpportunityCompetitors.Where(x => x.Id == competitor.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Opportunity competitor was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("api/opportunity-competitors/{id:guid}")]
    [HasPermission("OpportunityCompetitors.Update")]
    public async Task<IActionResult> UpdateCompetitor(Guid id, UpsertOpportunityCompetitorRequestDto dto)
    {
        var competitor = await _dbContext.OpportunityCompetitors.FirstOrDefaultAsync(x => x.Id == id);
        if (competitor is null)
        {
            return NotFound();
        }

        var validationError = await ValidateCompetitorAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyCompetitorValues(competitor, dto);
        if (competitor.IsPrimaryCompetitor)
        {
            await ClearPrimaryCompetitorsAsync(competitor.OpportunityId, competitor.Id);
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("api/opportunity-competitors/{id:guid}/set-primary")]
    [HasPermission("OpportunityCompetitors.SetPrimary")]
    public async Task<IActionResult> SetPrimary(Guid id)
    {
        var competitor = await _dbContext.OpportunityCompetitors.FirstOrDefaultAsync(x => x.Id == id);
        if (competitor is null)
        {
            return NotFound();
        }

        await ClearPrimaryCompetitorsAsync(competitor.OpportunityId, competitor.Id);
        competitor.IsPrimaryCompetitor = true;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/opportunity-competitors/{id:guid}")]
    [HasPermission("OpportunityCompetitors.Delete")]
    public async Task<IActionResult> DeleteCompetitor(Guid id)
    {
        var competitor = await _dbContext.OpportunityCompetitors.FirstOrDefaultAsync(x => x.Id == id);
        if (competitor is null)
        {
            return NotFound();
        }

        var linkedOpportunities = await _dbContext.Opportunities
            .Where(x => x.LostToCompetitorId == id)
            .ToListAsync();
        foreach (var opportunity in linkedOpportunities)
        {
            opportunity.LostToCompetitorId = null;
        }

        competitor.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task ClearPrimaryCompetitorsAsync(Guid opportunityId, Guid keepCompetitorId)
    {
        var primaries = await _dbContext.OpportunityCompetitors
            .Where(x => x.OpportunityId == opportunityId && x.Id != keepCompetitorId && x.IsPrimaryCompetitor)
            .ToListAsync();

        foreach (var primary in primaries)
        {
            primary.IsPrimaryCompetitor = false;
        }
    }

    private async Task<string?> ValidateCompetitorAsync(UpsertOpportunityCompetitorRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompetitorName))
        {
            return "Competitor name is required.";
        }

        if (dto.ThreatLevelId.HasValue &&
            !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ThreatLevelId.Value && x.LookupCategory.Code == "COMPETITOR_THREAT_LEVEL"))
        {
            return "Threat level is invalid.";
        }

        return null;
    }

    private static void ApplyCompetitorValues(OpportunityCompetitor competitor, UpsertOpportunityCompetitorRequestDto dto)
    {
        competitor.CompetitorName = dto.CompetitorName.Trim();
        competitor.Strengths = TrimToNull(dto.Strengths);
        competitor.Weaknesses = TrimToNull(dto.Weaknesses);
        competitor.ThreatLevelId = dto.ThreatLevelId;
        competitor.IsPrimaryCompetitor = dto.IsPrimaryCompetitor;
        competitor.Notes = TrimToNull(dto.Notes);
    }

    private static IQueryable<OpportunityCompetitorDto> ProjectCompetitors(IQueryable<OpportunityCompetitor> query)
    {
        return query.Select(x => new OpportunityCompetitorDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            CompetitorName = x.CompetitorName,
            Strengths = x.Strengths,
            Weaknesses = x.Weaknesses,
            ThreatLevelId = x.ThreatLevelId,
            ThreatLevelName = x.ThreatLevel != null ? x.ThreatLevel.Name : null,
            IsPrimaryCompetitor = x.IsPrimaryCompetitor,
            Notes = x.Notes
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
