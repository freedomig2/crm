using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/security-policies")]
public class SecurityPoliciesController : ControllerBase
{
    private const string SecurityScopeTypeCategoryCode = "SECURITY_SCOPE_TYPE";

    private readonly AppDbContext _dbContext;

    public SecurityPoliciesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("SecurityPolicies.View")]
    public async Task<ActionResult<PagedResult<SecurityPolicyDto>>> GetPolicies([FromQuery] SecurityPolicyFilterDto query)
    {
        var policies = _dbContext.SecurityPolicies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.ScopeTypeCode))
        {
            var scopeCode = query.ScopeTypeCode.Trim().ToUpperInvariant();
            policies = policies.Where(x => x.ScopeType.Code == scopeCode);
        }

        if (query.MaskSensitiveFields.HasValue)
        {
            policies = policies.Where(x => x.MaskSensitiveFields == query.MaskSensitiveFields.Value);
        }

        if (query.IsActive.HasValue)
        {
            policies = policies.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            policies = policies.Where(x =>
                x.EntityName.ToLower().Contains(search) ||
                x.ScopeType.Name.ToLower().Contains(search) ||
                (x.SensitiveFieldList ?? string.Empty).ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        policies = policies.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            policies = policies.OrderBy(x => x.EntityName);
        }

        return Ok(await ProjectPolicies(policies).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("SecurityPolicies.View")]
    public async Task<ActionResult<SecurityPolicyDto>> GetPolicy(Guid id)
    {
        var item = await ProjectPolicies(_dbContext.SecurityPolicies.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("SecurityPolicies.Create")]
    public async Task<ActionResult<SecurityPolicyDto>> CreatePolicy(UpsertSecurityPolicyRequestDto dto)
    {
        var (validationError, scopeTypeId) = await ValidatePolicyAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var entityName = dto.EntityName.Trim().ToUpperInvariant();
        if (await _dbContext.SecurityPolicies.AnyAsync(x => x.EntityName == entityName))
        {
            return BadRequest("A security policy for this entity already exists.");
        }

        var item = new SecurityPolicy();
        ApplyPolicyValues(item, dto, scopeTypeId!.Value);

        _dbContext.SecurityPolicies.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectPolicies(_dbContext.SecurityPolicies.Where(x => x.Id == item.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Security policy was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("SecurityPolicies.Update")]
    public async Task<IActionResult> UpdatePolicy(Guid id, UpsertSecurityPolicyRequestDto dto)
    {
        var item = await _dbContext.SecurityPolicies.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var (validationError, scopeTypeId) = await ValidatePolicyAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var entityName = dto.EntityName.Trim().ToUpperInvariant();
        if (await _dbContext.SecurityPolicies.AnyAsync(x => x.Id != id && x.EntityName == entityName))
        {
            return BadRequest("A security policy for this entity already exists.");
        }

        ApplyPolicyValues(item, dto, scopeTypeId!.Value);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("SecurityPolicies.Delete")]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        var item = await _dbContext.SecurityPolicies.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<(string? ValidationError, Guid? ScopeTypeId)> ValidatePolicyAsync(UpsertSecurityPolicyRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EntityName))
        {
            return ("Entity name is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.ScopeTypeCode))
        {
            return ("Scope type code is required.", null);
        }

        var scopeCode = dto.ScopeTypeCode.Trim().ToUpperInvariant();
        var scopeTypeId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == SecurityScopeTypeCategoryCode && x.Code == scopeCode && x.IsActive)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!scopeTypeId.HasValue)
        {
            return ("Scope type is invalid. Allowed values: ALL, OWNER, TEAM, OWNER_OR_TEAM.", null);
        }

        return (null, scopeTypeId.Value);
    }

    private static void ApplyPolicyValues(SecurityPolicy policy, UpsertSecurityPolicyRequestDto dto, Guid scopeTypeId)
    {
        policy.EntityName = dto.EntityName.Trim().ToUpperInvariant();
        policy.ScopeTypeId = scopeTypeId;
        policy.MaskSensitiveFields = dto.MaskSensitiveFields;
        policy.SensitiveFieldList = string.IsNullOrWhiteSpace(dto.SensitiveFieldList) ? null : dto.SensitiveFieldList.Trim();
        policy.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        policy.IsActive = dto.IsActive;
    }

    private static IQueryable<SecurityPolicyDto> ProjectPolicies(IQueryable<SecurityPolicy> query)
    {
        return query.Select(x => new SecurityPolicyDto
        {
            Id = x.Id,
            EntityName = x.EntityName,
            ScopeTypeName = x.ScopeType.Name,
            ScopeTypeCode = x.ScopeType.Code ?? string.Empty,
            MaskSensitiveFields = x.MaskSensitiveFields,
            SensitiveFieldList = x.SensitiveFieldList,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }
}
