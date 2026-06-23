using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class IntegrationConnectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string DirectionCode { get; set; } = string.Empty;
    public string DirectionName { get; set; } = string.Empty;
    public string AuthTypeCode { get; set; } = string.Empty;
    public string AuthTypeName { get; set; } = string.Empty;
    public string? EndpointUrl { get; set; }
    public string? ApiKeyReference { get; set; }
    public string? LastSyncStatusCode { get; set; }
    public string? LastSyncStatusName { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class IntegrationSyncRunDto
{
    public Guid Id { get; set; }
    public Guid IntegrationConnectionId { get; set; }
    public string IntegrationConnectionName { get; set; } = string.Empty;
    public string IntegrationConnectionCode { get; set; } = string.Empty;
    public string TriggerTypeCode { get; set; } = string.Empty;
    public string TriggerTypeName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IntegrationConnectionFilterDto : ListQueryDto
{
    public string? ProviderCode { get; set; }
    public string? DirectionCode { get; set; }
    public bool? IsActive { get; set; }
}

public class IntegrationSyncRunFilterDto : ListQueryDto
{
    public string? ConnectionCode { get; set; }
    public string? StatusCode { get; set; }
    public string? TriggerTypeCode { get; set; }
}

public class UpsertIntegrationConnectionRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string ProviderCode { get; set; } = string.Empty;

    [Required]
    public string DirectionCode { get; set; } = string.Empty;

    [Required]
    public string AuthTypeCode { get; set; } = string.Empty;

    [Url]
    public string? EndpointUrl { get; set; }

    public string? ApiKeyReference { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class IntegrationActionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
