using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class NumberSequenceDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string SequenceCode { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string Separator { get; set; } = "-";
    public long CurrentNumber { get; set; }
    public long NextNumber { get; set; }
    public int MinimumDigits { get; set; }
    public Guid? ResetFrequencyId { get; set; }
    public string? ResetFrequencyName { get; set; }
    public DateTime? LastResetDate { get; set; }
    public bool IncludeYear { get; set; }
    public bool IncludeMonth { get; set; }
    public bool IncludeDay { get; set; }
    public string? FormatPreview { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertNumberSequenceRequestDto
{
    [Required]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    public string SequenceCode { get; set; } = string.Empty;

    [Required]
    public string Prefix { get; set; } = string.Empty;

    public string? Suffix { get; set; }
    public string Separator { get; set; } = "-";
    public long CurrentNumber { get; set; }
    public long NextNumber { get; set; } = 1;
    public int MinimumDigits { get; set; } = 6;
    public Guid? ResetFrequencyId { get; set; }
    public DateTime? LastResetDate { get; set; }
    public bool IncludeYear { get; set; }
    public bool IncludeMonth { get; set; }
    public bool IncludeDay { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class NumberSequenceFilterDto : ListQueryDto
{
    public Guid? ResetFrequencyId { get; set; }
    public bool? IsActive { get; set; }
}

public class NumberSequencePreviewDto
{
    public string Preview { get; set; } = string.Empty;
}
