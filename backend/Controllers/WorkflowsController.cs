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
[Route("api/workflows")]
public class WorkflowsController : ControllerBase
{
    private const string WorkflowTypeCategoryCode = "WORKFLOW_TYPE";
    private const string WorkflowStatusCategoryCode = "WORKFLOW_STATUS";
    private static readonly Regex WorkflowCodePattern = new("^[A-Z0-9_]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;

    public WorkflowsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("Workflows.View")]
    public async Task<ActionResult<PagedResult<WorkflowDto>>> GetWorkflows([FromQuery] WorkflowFilterDto query)
    {
        var workflows = _dbContext.Workflows.AsQueryable();

        if (query.WorkflowTypeId.HasValue)
        {
            workflows = workflows.Where(x => x.WorkflowTypeId == query.WorkflowTypeId.Value);
        }

        if (query.WorkflowStatusId.HasValue)
        {
            workflows = workflows.Where(x => x.WorkflowStatusId == query.WorkflowStatusId.Value);
        }

        if (query.IsSystem.HasValue)
        {
            workflows = workflows.Where(x => x.IsSystem == query.IsSystem.Value);
        }

        if (query.IsActive.HasValue)
        {
            workflows = workflows.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            workflows = workflows.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.TriggerEntity.ToLower().Contains(search) ||
                x.TriggerEvent.ToLower().Contains(search) ||
                x.WorkflowType.Name.ToLower().Contains(search) ||
                x.WorkflowStatus.Name.ToLower().Contains(search));
        }

        workflows = workflows.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            workflows = workflows.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        }

        return Ok(await ProjectWorkflows(workflows).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Workflows.View")]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(Guid id)
    {
        var item = await ProjectWorkflows(_dbContext.Workflows.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Workflows.Create")]
    public async Task<ActionResult<WorkflowDto>> CreateWorkflow(UpsertWorkflowRequestDto dto)
    {
        var validationError = await ValidateWorkflowAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.Workflows.AnyAsync(x => x.Code == normalizedCode))
        {
            return BadRequest("Workflow code already exists.");
        }

        var item = new Workflow();
        ApplyWorkflowValues(item, dto);

        _dbContext.Workflows.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectWorkflows(_dbContext.Workflows.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Workflow was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Workflows.Update")]
    public async Task<IActionResult> UpdateWorkflow(Guid id, UpsertWorkflowRequestDto dto)
    {
        var item = await _dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var validationError = await ValidateWorkflowAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();
        if (await _dbContext.Workflows.AnyAsync(x => x.Id != id && x.Code == normalizedCode))
        {
            return BadRequest("Workflow code already exists.");
        }

        ApplyWorkflowValues(item, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Workflows.Delete")]
    public async Task<IActionResult> DeleteWorkflow(Guid id)
    {
        var item = await _dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Workflows.View")]
    public async Task<ActionResult<WorkflowLookupDto>> GetLookup()
    {
        return Ok(new WorkflowLookupDto
        {
            WorkflowTypes = await GetLookupOptionsAsync(WorkflowTypeCategoryCode),
            WorkflowStatuses = await GetLookupOptionsAsync(WorkflowStatusCategoryCode),
        });
    }

    private async Task<string?> ValidateWorkflowAsync(Guid? workflowId, UpsertWorkflowRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Workflow name is required.";
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return "Workflow code is required.";
        }

        if (normalizedCode != dto.Code.Trim() || normalizedCode.Contains(' ') || !WorkflowCodePattern.IsMatch(normalizedCode))
        {
            return "Workflow code must be uppercase with no spaces.";
        }

        if (string.IsNullOrWhiteSpace(dto.TriggerEntity))
        {
            return "Trigger entity is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.TriggerEvent))
        {
            return "Trigger event is required.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.WorkflowTypeId && x.LookupCategory.Code == WorkflowTypeCategoryCode))
        {
            return "Workflow type is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.WorkflowStatusId && x.LookupCategory.Code == WorkflowStatusCategoryCode))
        {
            return "Workflow status is invalid.";
        }

        if (workflowId.HasValue)
        {
            if (!await _dbContext.Workflows.AnyAsync(x => x.Id == workflowId.Value))
            {
                return "Workflow was not found.";
            }
        }

        return null;
    }

    private static void ApplyWorkflowValues(Workflow item, UpsertWorkflowRequestDto dto)
    {
        item.Name = dto.Name.Trim();
        item.Code = dto.Code.Trim().ToUpperInvariant();
        item.Description = TrimToNull(dto.Description);
        item.WorkflowTypeId = dto.WorkflowTypeId;
        item.WorkflowStatusId = dto.WorkflowStatusId;
        item.TriggerEntity = dto.TriggerEntity.Trim();
        item.TriggerEvent = dto.TriggerEvent.Trim();
        item.IsDefault = dto.IsDefault;
        item.IsSystem = dto.IsSystem;
        item.Version = dto.Version;
        item.SortOrder = dto.SortOrder;
        item.IsActive = dto.IsActive;
    }

    private IQueryable<WorkflowDto> ProjectWorkflows(IQueryable<Workflow> query)
    {
        return query.Select(x => new WorkflowDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Description = x.Description,
            WorkflowTypeId = x.WorkflowTypeId,
            WorkflowTypeName = x.WorkflowType.Name,
            WorkflowTypeCode = x.WorkflowType.Code,
            WorkflowStatusId = x.WorkflowStatusId,
            WorkflowStatusName = x.WorkflowStatus.Name,
            WorkflowStatusCode = x.WorkflowStatus.Code,
            TriggerEntity = x.TriggerEntity,
            TriggerEvent = x.TriggerEvent,
            IsDefault = x.IsDefault,
            IsSystem = x.IsSystem,
            Version = x.Version,
            SortOrder = x.SortOrder,
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

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
