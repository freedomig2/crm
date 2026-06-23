using CRM.Domain.Common;

namespace backend.Entities;

public class IntegrationConnection : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public LookupValue Provider { get; set; } = default!;
    public Guid DirectionId { get; set; }
    public LookupValue Direction { get; set; } = default!;
    public Guid AuthTypeId { get; set; }
    public LookupValue AuthType { get; set; } = default!;
    public string? EndpointUrl { get; set; }
    public string? ApiKeyReference { get; set; }
    public Guid? LastSyncStatusId { get; set; }
    public LookupValue? LastSyncStatus { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? Description { get; set; }

    public ICollection<IntegrationSyncRun> SyncRuns { get; set; } = new List<IntegrationSyncRun>();
}
