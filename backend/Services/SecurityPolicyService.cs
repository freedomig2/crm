using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public interface ISecurityPolicyService
{
    Task<SecurityPolicyContext?> GetPolicyAsync(string entityName, CancellationToken cancellationToken = default);
    Task<IQueryable<Account>> ApplyAccountScopeAsync(IQueryable<Account> query, SecurityPolicyContext? policy, CancellationToken cancellationToken = default);
    Task<bool> CanAccessAccountAsync(Account account, SecurityPolicyContext? policy, CancellationToken cancellationToken = default);
    void ApplyAccountFieldMask(AccountDto account, SecurityPolicyContext? policy, ClaimsPrincipal user);
}

public sealed class SecurityPolicyContext
{
    public string EntityName { get; init; } = string.Empty;
    public string ScopeTypeCode { get; init; } = "ALL";
    public bool MaskSensitiveFields { get; init; }
    public string? SensitiveFieldList { get; init; }
}

public class SecurityPolicyService : ISecurityPolicyService
{
    private const string ViewSensitiveFieldsPermission = "SecurityPolicies.ViewSensitiveFields";

    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public SecurityPolicyService(AppDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    public async Task<SecurityPolicyContext?> GetPolicyAsync(string entityName, CancellationToken cancellationToken = default)
    {
        var normalized = entityName.Trim().ToUpperInvariant();

        return await _dbContext.SecurityPolicies
            .Where(x => x.EntityName == normalized && x.IsActive)
            .Select(x => new SecurityPolicyContext
            {
                EntityName = x.EntityName,
                ScopeTypeCode = x.ScopeType.Code,
                MaskSensitiveFields = x.MaskSensitiveFields,
                SensitiveFieldList = x.SensitiveFieldList,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IQueryable<Account>> ApplyAccountScopeAsync(IQueryable<Account> query, SecurityPolicyContext? policy, CancellationToken cancellationToken = default)
    {
        if (policy is null)
        {
            return query;
        }

        var scopeCode = (policy.ScopeTypeCode ?? "ALL").Trim().ToUpperInvariant();
        if (scopeCode == "ALL")
        {
            return query;
        }

        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
        {
            return query.Where(_ => false);
        }

        if (scopeCode == "OWNER")
        {
            return query.Where(x => x.OwnerUserId == userId.Value);
        }

        var teamIds = await GetCurrentUserTeamIdsAsync(cancellationToken);
        if (scopeCode == "TEAM")
        {
            return query.Where(x => x.OwnerTeamId.HasValue && teamIds.Contains(x.OwnerTeamId.Value));
        }

        if (scopeCode == "OWNER_OR_TEAM")
        {
            return query.Where(x => x.OwnerUserId == userId.Value || (x.OwnerTeamId.HasValue && teamIds.Contains(x.OwnerTeamId.Value)));
        }

        return query;
    }

    public async Task<bool> CanAccessAccountAsync(Account account, SecurityPolicyContext? policy, CancellationToken cancellationToken = default)
    {
        if (policy is null)
        {
            return true;
        }

        var scopeCode = (policy.ScopeTypeCode ?? "ALL").Trim().ToUpperInvariant();
        if (scopeCode == "ALL")
        {
            return true;
        }

        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
        {
            return false;
        }

        if (scopeCode == "OWNER")
        {
            return account.OwnerUserId == userId.Value;
        }

        var teamIds = await GetCurrentUserTeamIdsAsync(cancellationToken);
        var inTeamScope = account.OwnerTeamId.HasValue && teamIds.Contains(account.OwnerTeamId.Value);

        if (scopeCode == "TEAM")
        {
            return inTeamScope;
        }

        if (scopeCode == "OWNER_OR_TEAM")
        {
            return account.OwnerUserId == userId.Value || inTeamScope;
        }

        return true;
    }

    public void ApplyAccountFieldMask(AccountDto account, SecurityPolicyContext? policy, ClaimsPrincipal user)
    {
        if (policy is null || !policy.MaskSensitiveFields)
        {
            return;
        }

        if (user.HasClaim("permission", ViewSensitiveFieldsPermission))
        {
            return;
        }

        var fields = ParseFields(policy.SensitiveFieldList);

        if (fields.Contains("TaxNumber"))
        {
            account.TaxNumber = null;
        }

        if (fields.Contains("RegistrationNumber"))
        {
            account.RegistrationNumber = null;
        }

        if (fields.Contains("AnnualRevenue"))
        {
            account.AnnualRevenue = null;
        }

        if (fields.Contains("AlternatePhone"))
        {
            account.AlternatePhone = null;
        }

        if (fields.Contains("MainPhone"))
        {
            account.MainPhone = null;
        }
    }

    private async Task<HashSet<Guid>> GetCurrentUserTeamIdsAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
        {
            return new HashSet<Guid>();
        }

        var teamIds = await _dbContext.UserTeams
            .Where(x => x.UserId == userId.Value)
            .Select(x => x.TeamId)
            .ToListAsync(cancellationToken);

        return teamIds.ToHashSet();
    }

    private static HashSet<string> ParseFields(string? fieldList)
    {
        if (string.IsNullOrWhiteSpace(fieldList))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "TaxNumber",
                "RegistrationNumber",
                "AnnualRevenue",
            };
        }

        return fieldList
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
