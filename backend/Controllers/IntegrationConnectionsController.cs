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
[Route("api/integration-connections")]
public class IntegrationConnectionsController : ControllerBase
{
    private const string ProviderCategoryCode = "INTEGRATION_PROVIDER";
    private const string DirectionCategoryCode = "INTEGRATION_DIRECTION";
    private const string AuthTypeCategoryCode = "INTEGRATION_AUTH_TYPE";

    private static readonly Regex CodePattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;
    private readonly IIntegrationExecutionService _integrationExecutionService;

    public IntegrationConnectionsController(AppDbContext dbContext, IIntegrationExecutionService integrationExecutionService)
    {
        _dbContext = dbContext;
        _integrationExecutionService = integrationExecutionService;
    }

    [HttpGet]
    [HasPermission("Integrations.View")]
    public async Task<ActionResult<PagedResult<IntegrationConnectionDto>>> GetConnections([FromQuery] IntegrationConnectionFilterDto query)
    {
        var connections = _dbContext.IntegrationConnections.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.ProviderCode))
        {
            var providerCode = query.ProviderCode.Trim().ToUpperInvariant();
            connections = connections.Where(x => x.Provider.Code == providerCode);
        }

        if (!string.IsNullOrWhiteSpace(query.DirectionCode))
        {
            var directionCode = query.DirectionCode.Trim().ToUpperInvariant();
            connections = connections.Where(x => x.Direction.Code == directionCode);
        }

        if (query.IsActive.HasValue)
        {
            connections = connections.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            connections = connections.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                x.Provider.Name.ToLower().Contains(search) ||
                x.Direction.Name.ToLower().Contains(search) ||
                x.AuthType.Name.ToLower().Contains(search) ||
                (x.EndpointUrl ?? string.Empty).ToLower().Contains(search));
        }

        connections = connections.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            connections = connections.OrderBy(x => x.Name);
        }

        return Ok(await ProjectConnections(connections).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Integrations.View")]
    public async Task<ActionResult<IntegrationConnectionDto>> GetConnection(Guid id)
    {
        var connection = await ProjectConnections(_dbContext.IntegrationConnections.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return connection is null ? NotFound() : Ok(connection);
    }

    [HttpPost]
    [HasPermission("Integrations.Create")]
    public async Task<ActionResult<IntegrationConnectionDto>> CreateConnection(UpsertIntegrationConnectionRequestDto dto)
    {
        var validationError = await ValidateDtoAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.IntegrationConnections.AnyAsync(x => x.Code == code))
        {
            return BadRequest("Integration connection code already exists.");
        }

        var connection = new IntegrationConnection();
        var applyError = await ApplyValuesAsync(connection, dto);
        if (applyError is not null)
        {
            return BadRequest(applyError);
        }

        _dbContext.IntegrationConnections.Add(connection);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectConnections(_dbContext.IntegrationConnections.Where(x => x.Id == connection.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Integration connection was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Integrations.Update")]
    public async Task<IActionResult> UpdateConnection(Guid id, UpsertIntegrationConnectionRequestDto dto)
    {
        var connection = await _dbContext.IntegrationConnections.FirstOrDefaultAsync(x => x.Id == id);
        if (connection is null)
        {
            return NotFound();
        }

        var validationError = await ValidateDtoAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.IntegrationConnections.AnyAsync(x => x.Id != id && x.Code == code))
        {
            return BadRequest("Integration connection code already exists.");
        }

        var applyError = await ApplyValuesAsync(connection, dto);
        if (applyError is not null)
        {
            return BadRequest(applyError);
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Integrations.Delete")]
    public async Task<IActionResult> DeleteConnection(Guid id)
    {
        var connection = await _dbContext.IntegrationConnections.FirstOrDefaultAsync(x => x.Id == id);
        if (connection is null)
        {
            return NotFound();
        }

        connection.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/test")]
    [HasPermission("Integrations.TestConnection")]
    public async Task<ActionResult<IntegrationActionResultDto>> TestConnection(Guid id)
    {
        var connection = await _dbContext.IntegrationConnections
            .Include(x => x.Provider)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (connection is null)
        {
            return NotFound();
        }

        var result = await _integrationExecutionService.TestConnectionAsync(connection);
        return Ok(result);
    }

    [HttpPost("{id:guid}/sync")]
    [HasPermission("Integrations.RunSync")]
    public async Task<ActionResult<IntegrationActionResultDto>> RunSync(Guid id)
    {
        var connection = await _dbContext.IntegrationConnections
            .Include(x => x.Provider)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (connection is null)
        {
            return NotFound();
        }

        if (!connection.IsActive)
        {
            return BadRequest("Inactive integration connections cannot run sync.");
        }

        var runningStatus = await _integrationExecutionService.GetStatusByCodeAsync("RUNNING");
        var successStatus = await _integrationExecutionService.GetStatusByCodeAsync("SUCCESS");
        var failedStatus = await _integrationExecutionService.GetStatusByCodeAsync("FAILED");
        var manualTrigger = await _integrationExecutionService.GetTriggerTypeByCodeAsync("MANUAL");

        if (runningStatus is null || successStatus is null || failedStatus is null || manualTrigger is null)
        {
            return BadRequest("Integration lookup data is missing. Seed data must include status and trigger values.");
        }

        var run = new IntegrationSyncRun
        {
            IntegrationConnectionId = connection.Id,
            TriggerTypeId = manualTrigger.Id,
            StatusId = runningStatus.Id,
            StartedAt = DateTime.UtcNow,
        };

        _dbContext.IntegrationSyncRuns.Add(run);
        await _dbContext.SaveChangesAsync();

        var syncResult = await _integrationExecutionService.RunSyncAsync(connection);

        run.StatusId = syncResult.Success ? successStatus.Id : failedStatus.Id;
        run.CompletedAt = DateTime.UtcNow;
        run.RecordsProcessed = syncResult.RecordsProcessed;
        run.ErrorMessage = syncResult.Success ? null : syncResult.Message;

        connection.LastSyncAt = run.CompletedAt;
        connection.LastSyncStatusId = run.StatusId;

        await _dbContext.SaveChangesAsync();

        return Ok(new IntegrationActionResultDto
        {
            Success = syncResult.Success,
            Message = syncResult.Message,
        });
    }

    private async Task<string?> ValidateDtoAsync(UpsertIntegrationConnectionRequestDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Connection name is required.";
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return "Connection code is required.";
        }

        if (code != dto.Code.Trim() || code.Contains(' ') || !CodePattern.IsMatch(code))
        {
            return "Connection code must be uppercase with no spaces.";
        }

        if (!await LookupCodeExistsAsync(ProviderCategoryCode, dto.ProviderCode))
        {
            return "Provider code is invalid.";
        }

        if (!await LookupCodeExistsAsync(DirectionCategoryCode, dto.DirectionCode))
        {
            return "Direction code is invalid.";
        }

        if (!await LookupCodeExistsAsync(AuthTypeCategoryCode, dto.AuthTypeCode))
        {
            return "Authentication type code is invalid.";
        }

        return null;
    }

    private async Task<string?> ApplyValuesAsync(IntegrationConnection connection, UpsertIntegrationConnectionRequestDto dto)
    {
        var providerId = await ResolveLookupIdAsync(ProviderCategoryCode, dto.ProviderCode);
        var directionId = await ResolveLookupIdAsync(DirectionCategoryCode, dto.DirectionCode);
        var authTypeId = await ResolveLookupIdAsync(AuthTypeCategoryCode, dto.AuthTypeCode);

        if (!providerId.HasValue || !directionId.HasValue || !authTypeId.HasValue)
        {
            return "One or more lookup codes are invalid.";
        }

        connection.Name = dto.Name.Trim();
        connection.Code = dto.Code.Trim().ToUpperInvariant();
        connection.ProviderId = providerId.Value;
        connection.DirectionId = directionId.Value;
        connection.AuthTypeId = authTypeId.Value;
        connection.EndpointUrl = string.IsNullOrWhiteSpace(dto.EndpointUrl) ? null : dto.EndpointUrl.Trim();
        connection.ApiKeyReference = string.IsNullOrWhiteSpace(dto.ApiKeyReference) ? null : dto.ApiKeyReference.Trim();
        connection.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        connection.IsActive = dto.IsActive;

        return null;
    }

    private Task<bool> LookupCodeExistsAsync(string categoryCode, string valueCode)
    {
        var normalized = valueCode.Trim().ToUpperInvariant();
        return _dbContext.LookupValues.AnyAsync(x => x.LookupCategory.Code == categoryCode && x.Code == normalized && x.IsActive);
    }

    private Task<Guid?> ResolveLookupIdAsync(string categoryCode, string valueCode)
    {
        var normalized = valueCode.Trim().ToUpperInvariant();
        return _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.Code == normalized && x.IsActive)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static IQueryable<IntegrationConnectionDto> ProjectConnections(IQueryable<IntegrationConnection> query)
    {
        return query.Select(x => new IntegrationConnectionDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            ProviderCode = x.Provider.Code ?? string.Empty,
            ProviderName = x.Provider.Name,
            DirectionCode = x.Direction.Code ?? string.Empty,
            DirectionName = x.Direction.Name,
            AuthTypeCode = x.AuthType.Code ?? string.Empty,
            AuthTypeName = x.AuthType.Name,
            EndpointUrl = x.EndpointUrl,
            ApiKeyReference = x.ApiKeyReference,
            LastSyncStatusCode = x.LastSyncStatus != null ? x.LastSyncStatus.Code : null,
            LastSyncStatusName = x.LastSyncStatus != null ? x.LastSyncStatus.Name : null,
            LastSyncAt = x.LastSyncAt,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }
}
