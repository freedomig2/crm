using CRM.Domain.Common;

namespace backend.Entities;

public class CustomFieldDefinition : ActivatableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid DataTypeId { get; set; }
    public LookupValue DataType { get; set; } = default!;
    public bool IsRequired { get; set; }
    public bool IsIndexed { get; set; }
    public string? DefaultValue { get; set; }
    public string? OptionsJson { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
}
