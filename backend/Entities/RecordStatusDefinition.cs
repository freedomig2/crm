using CRM.Domain.Common;

namespace backend.Entities;

public class RecordStatusDefinition : ActivatableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsClosedState { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
}
