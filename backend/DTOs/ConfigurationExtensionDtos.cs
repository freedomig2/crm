using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CustomFieldDefinitionDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataTypeCode { get; set; } = string.Empty;
    public string DataTypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsIndexed { get; set; }
    public string? DefaultValue { get; set; }
    public string? OptionsJson { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RecordStatusDefinitionDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsClosedState { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CustomFieldDefinitionFilterDto : ListQueryDto
{
    public string? EntityName { get; set; }
    public string? DataTypeCode { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsActive { get; set; }
}

public class RecordStatusDefinitionFilterDto : ListQueryDto
{
    public string? EntityName { get; set; }
    public bool? IsClosedState { get; set; }
    public bool? IsActive { get; set; }
}

public class UpsertCustomFieldDefinitionRequestDto
{
    [Required]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    public string FieldKey { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string DataTypeCode { get; set; } = string.Empty;

    public bool IsRequired { get; set; }
    public bool IsIndexed { get; set; }
    public string? DefaultValue { get; set; }
    public string? OptionsJson { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpsertRecordStatusDefinitionRequestDto
{
    [Required]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    public string StatusCode { get; set; } = string.Empty;

    [Required]
    public string StatusName { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
    public bool IsClosedState { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
