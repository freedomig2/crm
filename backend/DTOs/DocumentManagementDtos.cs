using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid DocumentCategoryId { get; set; }
    public string DocumentCategoryName { get; set; } = string.Empty;
    public string DocumentCategoryCode { get; set; } = string.Empty;
    public Guid DocumentStatusId { get; set; }
    public string DocumentStatusName { get; set; } = string.Empty;
    public string DocumentStatusCode { get; set; } = string.Empty;
    public Guid? AccountId { get; set; }
    public string? AccountName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public Guid? LeadId { get; set; }
    public string? LeadTopic { get; set; }
    public Guid? OpportunityId { get; set; }
    public string? OpportunityTopic { get; set; }
    public Guid? CaseId { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsConfidential { get; set; }
    public int CurrentVersion { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? ChangeSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DocumentFilterDto : ListQueryDto
{
    public Guid? DocumentCategoryId { get; set; }
    public Guid? DocumentStatusId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? CaseId { get; set; }
    public bool? IsConfidential { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveDateFrom { get; set; }
    public DateTime? EffectiveDateTo { get; set; }
    public DateTime? ExpiryDateFrom { get; set; }
    public DateTime? ExpiryDateTo { get; set; }
}

public class DocumentVersionFilterDto : ListQueryDto
{
    public Guid? DocumentId { get; set; }
}

public class UpsertDocumentRequestDto
{
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Range(0, long.MaxValue)]
    public long FileSizeBytes { get; set; }

    [Required]
    public string StoragePath { get; set; } = string.Empty;

    [Required]
    public Guid DocumentCategoryId { get; set; }

    [Required]
    public Guid DocumentStatusId { get; set; }

    public Guid? AccountId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? CaseId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsConfidential { get; set; }
    [Range(1, int.MaxValue)]
    public int? CurrentVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class AddDocumentVersionRequestDto
{
    [Range(1, int.MaxValue)]
    public int? VersionNumber { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Range(0, long.MaxValue)]
    public long FileSizeBytes { get; set; }

    [Required]
    public string StoragePath { get; set; } = string.Empty;

    public string? ChangeSummary { get; set; }
}

public class DocumentLookupDto
{
    public IReadOnlyCollection<LookupOptionDto> DocumentCategories { get; set; } = Array.Empty<LookupOptionDto>();
    public IReadOnlyCollection<LookupOptionDto> DocumentStatuses { get; set; } = Array.Empty<LookupOptionDto>();
}
