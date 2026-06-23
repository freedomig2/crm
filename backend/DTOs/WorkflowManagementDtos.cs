using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class WorkflowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkflowTypeId { get; set; }
    public string WorkflowTypeName { get; set; } = string.Empty;
    public string WorkflowTypeCode { get; set; } = string.Empty;
    public Guid WorkflowStatusId { get; set; }
    public string WorkflowStatusName { get; set; } = string.Empty;
    public string WorkflowStatusCode { get; set; } = string.Empty;
    public string TriggerEntity { get; set; } = string.Empty;
    public string TriggerEvent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public int Version { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WorkflowFilterDto : ListQueryDto
{
    public Guid? WorkflowTypeId { get; set; }
    public Guid? WorkflowStatusId { get; set; }
    public bool? IsSystem { get; set; }
    public bool? IsActive { get; set; }
}

public class UpsertWorkflowRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public Guid WorkflowTypeId { get; set; }

    [Required]
    public Guid WorkflowStatusId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TriggerEntity { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TriggerEvent { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }

    [Range(1, int.MaxValue)]
    public int Version { get; set; } = 1;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public class WorkflowLookupDto
{
    public IReadOnlyCollection<LookupOptionDto> WorkflowTypes { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> WorkflowStatuses { get; set; } = Array.Empty<LookupOptionDto>();
}
