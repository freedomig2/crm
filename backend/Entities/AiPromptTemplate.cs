using CRM.Domain.Common;

namespace backend.Entities;

public class AiPromptTemplate : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string UseCaseCode { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int Version { get; set; } = 1;
}
