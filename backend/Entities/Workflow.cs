using CRM.Domain.Common;

namespace backend.Entities;

public class Workflow : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkflowTypeId { get; set; }
    public LookupValue WorkflowType { get; set; } = default!;
    public Guid WorkflowStatusId { get; set; }
    public LookupValue WorkflowStatus { get; set; } = default!;
    public string TriggerEntity { get; set; } = string.Empty;
    public string TriggerEvent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public int Version { get; set; } = 1;
    public int SortOrder { get; set; }
}
