namespace backend.Services;

public interface IAuditService
{
    Task LogAsync(
        string entityName,
        string entityId,
        string action,
        string? oldValues = null,
        string? newValues = null,
        Guid? userId = null);
}
