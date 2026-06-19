using CRM.Domain.Common;

namespace backend.Entities;

public class NumberSequence : ActivatableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string SequenceCode { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string Separator { get; set; } = "-";
    public long CurrentNumber { get; set; }
    public long NextNumber { get; set; } = 1;
    public int MinimumDigits { get; set; } = 6;
    public Guid? ResetFrequencyId { get; set; }
    public LookupValue? ResetFrequency { get; set; }
    public DateTime? LastResetDate { get; set; }
    public bool IncludeYear { get; set; }
    public bool IncludeMonth { get; set; }
    public bool IncludeDay { get; set; }
    public string? FormatPreview { get; set; }
    public string? Description { get; set; }
}
