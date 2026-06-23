using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/integration-sync-runs")]
public class IntegrationSyncRunsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public IntegrationSyncRunsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("IntegrationSyncRuns.View")]
    public async Task<ActionResult<PagedResult<IntegrationSyncRunDto>>> GetRuns([FromQuery] IntegrationSyncRunFilterDto query)
    {
        var runs = _dbContext.IntegrationSyncRuns.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.ConnectionCode))
        {
            var code = query.ConnectionCode.Trim().ToUpperInvariant();
            runs = runs.Where(x => x.IntegrationConnection.Code == code);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusCode))
        {
            var code = query.StatusCode.Trim().ToUpperInvariant();
            runs = runs.Where(x => x.Status.Code == code);
        }

        if (!string.IsNullOrWhiteSpace(query.TriggerTypeCode))
        {
            var code = query.TriggerTypeCode.Trim().ToUpperInvariant();
            runs = runs.Where(x => x.TriggerType.Code == code);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            runs = runs.Where(x =>
                x.IntegrationConnection.Name.ToLower().Contains(search) ||
                x.IntegrationConnection.Code.ToLower().Contains(search) ||
                x.Status.Name.ToLower().Contains(search) ||
                x.TriggerType.Name.ToLower().Contains(search) ||
                (x.ErrorMessage ?? string.Empty).ToLower().Contains(search));
        }

        runs = runs.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            runs = runs.OrderByDescending(x => x.StartedAt);
        }

        return Ok(await ProjectRuns(runs).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("IntegrationSyncRuns.View")]
    public async Task<ActionResult<IntegrationSyncRunDto>> GetRun(Guid id)
    {
        var run = await ProjectRuns(_dbContext.IntegrationSyncRuns.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return run is null ? NotFound() : Ok(run);
    }

    private static IQueryable<IntegrationSyncRunDto> ProjectRuns(IQueryable<IntegrationSyncRun> query)
    {
        return query.Select(x => new IntegrationSyncRunDto
        {
            Id = x.Id,
            IntegrationConnectionId = x.IntegrationConnectionId,
            IntegrationConnectionName = x.IntegrationConnection.Name,
            IntegrationConnectionCode = x.IntegrationConnection.Code,
            TriggerTypeCode = x.TriggerType.Code ?? string.Empty,
            TriggerTypeName = x.TriggerType.Name,
            StatusCode = x.Status.Code ?? string.Empty,
            StatusName = x.Status.Name,
            StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt,
            RecordsProcessed = x.RecordsProcessed,
            ErrorMessage = x.ErrorMessage,
            CreatedAt = x.CreatedAt,
        });
    }
}
