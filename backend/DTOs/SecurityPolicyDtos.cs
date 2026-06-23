using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SecurityPolicyDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string ScopeTypeName { get; set; } = string.Empty;
    public string ScopeTypeCode { get; set; } = string.Empty;
    public bool MaskSensitiveFields { get; set; }
    public string? SensitiveFieldList { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SecurityPolicyFilterDto : ListQueryDto
{
    public string? ScopeTypeCode { get; set; }
    public bool? MaskSensitiveFields { get; set; }
    public bool? IsActive { get; set; }
}

public class UpsertSecurityPolicyRequestDto
{
    [Required]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    public string ScopeTypeCode { get; set; } = string.Empty;

    public bool MaskSensitiveFields { get; set; }
    public string? SensitiveFieldList { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
