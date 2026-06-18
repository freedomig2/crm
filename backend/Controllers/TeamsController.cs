using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _auditService;

    public TeamsController(AppDbContext dbContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("Teams.View")]
    public async Task<ActionResult<PagedResult<TeamDto>>> GetTeams([FromQuery] ListQueryDto query)
    {
        var teamsQuery = _dbContext.Teams.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            teamsQuery = teamsQuery.Where(x => x.Name.ToLower().Contains(search) || (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        teamsQuery = teamsQuery.OrderByPropertyName(query.SortBy, query.SortDir);

        var projected = teamsQuery.Select(x => new TeamDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            OwnerUserId = x.OwnerUserId,
            IsActive = x.IsActive
        });

        return Ok(await projected.ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Teams.View")]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid id)
    {
        var team = await _dbContext.Teams
            .Where(x => x.Id == id)
            .Select(x => new TeamDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                OwnerUserId = x.OwnerUserId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost]
    [HasPermission("Teams.Create")]
    public async Task<ActionResult<TeamDto>> CreateTeam(UpsertTeamRequestDto dto)
    {
        var team = new Team
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerUserId = dto.OwnerUserId,
            IsActive = dto.IsActive
        };

        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Teams", team.Id.ToString(), "Create", newValues: team.Name);
        return Ok(new TeamDto { Id = team.Id, Name = team.Name, Description = team.Description, OwnerUserId = team.OwnerUserId, IsActive = team.IsActive });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Teams.Update")]
    public async Task<IActionResult> UpdateTeam(Guid id, UpsertTeamRequestDto dto)
    {
        var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == id);
        if (team is null)
        {
            return NotFound();
        }

        team.Name = dto.Name;
        team.Description = dto.Description;
        team.OwnerUserId = dto.OwnerUserId;
        team.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Teams", id.ToString(), "Update", newValues: dto.Name);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Teams.Delete")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        var team = await _dbContext.Teams.FirstOrDefaultAsync(x => x.Id == id);
        if (team is null)
        {
            return NotFound();
        }

        team.IsDeleted = true;

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Teams", id.ToString(), "Delete");
        return NoContent();
    }
}
