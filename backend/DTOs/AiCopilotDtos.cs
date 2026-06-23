using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class AiPromptTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UseCaseCode { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AiPromptTemplateFilterDto : ListQueryDto
{
    public string? UseCaseCode { get; set; }
    public bool? IsSystem { get; set; }
    public bool? IsActive { get; set; }
}

public class UpsertAiPromptTemplateRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string UseCaseCode { get; set; } = string.Empty;

    [Required]
    public string SystemPrompt { get; set; } = string.Empty;

    public bool IsSystem { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class AiDashboardSummaryDto
{
    public int OpenLeads { get; set; }
    public int OpenOpportunities { get; set; }
    public int OpenCases { get; set; }
    public IReadOnlyCollection<string> Insights { get; set; } = Array.Empty<string>();
}

public class AiRecommendationRequestDto
{
    [Required]
    public string ScenarioCode { get; set; } = string.Empty;

    public string? ContextText { get; set; }
}

public class AiRecommendationDto
{
    public string ScenarioCode { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Actions { get; set; } = Array.Empty<string>();
}
