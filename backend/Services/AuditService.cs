using backend.Data;
using backend.Entities;
using backend.Middleware;

namespace backend.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserContext _currentUserContext;

    public AuditService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _currentUserContext = currentUserContext;
    }

    public async Task LogAsync(string entityName, string entityId, string action, string? oldValues = null, string? newValues = null, Guid? userId = null)
    {
        var context = _httpContextAccessor.HttpContext;
        _dbContext.AuditLogs.Add(new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId ?? _currentUserContext.UserId,
            IpAddress = context?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context?.Request.Headers.UserAgent.ToString(),
            CreatedBy = _currentUserContext.UserEmail
        });

        await _dbContext.SaveChangesAsync();
    }
}
