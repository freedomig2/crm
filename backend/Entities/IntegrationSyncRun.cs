using CRM.Domain.Common;

namespace backend.Entities;

public class IntegrationSyncRun : BaseEntity
{
    public Guid IntegrationConnectionId { get; set; }
    public IntegrationConnection IntegrationConnection { get; set; } = default!;
    public Guid TriggerTypeId { get; set; }
    public LookupValue TriggerType { get; set; } = default!;
    public Guid StatusId { get; set; }
    public LookupValue Status { get; set; } = default!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
}
