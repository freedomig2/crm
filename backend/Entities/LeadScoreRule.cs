using CRM.Domain.Common;

namespace backend.Entities;

public class LeadScoreRule : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid RuleTypeId { get; set; }
    public LookupValue RuleType { get; set; } = default!;
    public string? FieldName { get; set; }
    public string? Operator { get; set; }
    public string? CompareValue { get; set; }
    public int ScoreValue { get; set; }
    public int SortOrder { get; set; }
}
