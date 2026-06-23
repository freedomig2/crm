using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private const string DocumentCategoryCode = "DOCUMENT_CATEGORY";
    private const string DocumentStatusCode = "DOCUMENT_STATUS";

    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public DocumentsController(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    [HttpGet]
    [HasPermission("Documents.View")]
    public async Task<ActionResult<PagedResult<DocumentDto>>> GetDocuments([FromQuery] DocumentFilterDto query)
    {
        var documents = _dbContext.Documents.AsQueryable();

        if (query.DocumentCategoryId.HasValue)
        {
            documents = documents.Where(x => x.DocumentCategoryId == query.DocumentCategoryId.Value);
        }

        if (query.DocumentStatusId.HasValue)
        {
            documents = documents.Where(x => x.DocumentStatusId == query.DocumentStatusId.Value);
        }

        if (query.AccountId.HasValue)
        {
            documents = documents.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.ContactId.HasValue)
        {
            documents = documents.Where(x => x.ContactId == query.ContactId.Value);
        }

        if (query.LeadId.HasValue)
        {
            documents = documents.Where(x => x.LeadId == query.LeadId.Value);
        }

        if (query.OpportunityId.HasValue)
        {
            documents = documents.Where(x => x.OpportunityId == query.OpportunityId.Value);
        }

        if (query.CaseId.HasValue)
        {
            documents = documents.Where(x => x.CaseId == query.CaseId.Value);
        }

        if (query.IsConfidential.HasValue)
        {
            documents = documents.Where(x => x.IsConfidential == query.IsConfidential.Value);
        }

        if (query.IsActive.HasValue)
        {
            documents = documents.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.EffectiveDateFrom.HasValue)
        {
            documents = documents.Where(x => x.EffectiveDate.HasValue && x.EffectiveDate >= query.EffectiveDateFrom.Value);
        }

        if (query.EffectiveDateTo.HasValue)
        {
            documents = documents.Where(x => x.EffectiveDate.HasValue && x.EffectiveDate <= query.EffectiveDateTo.Value);
        }

        if (query.ExpiryDateFrom.HasValue)
        {
            documents = documents.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate >= query.ExpiryDateFrom.Value);
        }

        if (query.ExpiryDateTo.HasValue)
        {
            documents = documents.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate <= query.ExpiryDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            documents = documents.Where(x =>
                x.DocumentNumber.ToLower().Contains(search) ||
                x.Title.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search) ||
                x.FileName.ToLower().Contains(search) ||
                x.DocumentCategory.Name.ToLower().Contains(search) ||
                x.DocumentStatus.Name.ToLower().Contains(search) ||
                (x.Account != null && x.Account.Name.ToLower().Contains(search)) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Lead != null && x.Lead.Topic.ToLower().Contains(search)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(search)) ||
                (x.Case != null && x.Case.CaseNumber.ToLower().Contains(search)));
        }

        documents = documents.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            documents = documents.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectDocuments(documents).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Documents.View")]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
    {
        var item = await ProjectDocuments(_dbContext.Documents.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [HasPermission("Documents.Create")]
    public async Task<ActionResult<DocumentDto>> CreateDocument(UpsertDocumentRequestDto dto)
    {
        var validationError = await ValidateDocumentAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var document = new Document();
        ApplyDocumentValues(document, dto, isCreate: true);

        if (string.IsNullOrWhiteSpace(document.DocumentNumber))
        {
            document.DocumentNumber = await _numberSequenceService.GenerateNextAsync("DOCUMENT");
        }

        if (await _dbContext.Documents.AnyAsync(x => x.DocumentNumber == document.DocumentNumber))
        {
            return BadRequest("Document number already exists.");
        }

        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectDocuments(_dbContext.Documents.Where(x => x.Id == document.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Document was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Documents.Update")]
    public async Task<IActionResult> UpdateDocument(Guid id, UpsertDocumentRequestDto dto)
    {
        var document = await _dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (document is null)
        {
            return NotFound();
        }

        var validationError = await ValidateDocumentAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyDocumentValues(document, dto, isCreate: false);
        if (string.IsNullOrWhiteSpace(document.DocumentNumber))
        {
            document.DocumentNumber = await _numberSequenceService.GenerateNextAsync("DOCUMENT");
        }

        if (await _dbContext.Documents.AnyAsync(x => x.Id != id && x.DocumentNumber == document.DocumentNumber))
        {
            return BadRequest("Document number already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Documents.Delete")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var document = await _dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (document is null)
        {
            return NotFound();
        }

        document.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Documents.View")]
    public async Task<ActionResult<DocumentLookupDto>> GetLookup()
    {
        return Ok(new DocumentLookupDto
        {
            DocumentCategories = await GetLookupOptionsAsync(DocumentCategoryCode),
            DocumentStatuses = await GetLookupOptionsAsync(DocumentStatusCode),
        });
    }

    [HttpGet("{documentId:guid}/versions")]
    [HasPermission("DocumentVersions.View")]
    public async Task<ActionResult<PagedResult<DocumentVersionDto>>> GetVersions(Guid documentId, [FromQuery] DocumentVersionFilterDto query)
    {
        if (!await _dbContext.Documents.AnyAsync(x => x.Id == documentId))
        {
            return NotFound();
        }

        query.DocumentId = documentId;
        var versions = _dbContext.DocumentVersions.AsQueryable();

        if (query.DocumentId.HasValue)
        {
            versions = versions.Where(x => x.DocumentId == query.DocumentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            versions = versions.Where(x =>
                x.FileName.ToLower().Contains(search) ||
                x.ContentType.ToLower().Contains(search) ||
                (x.ChangeSummary ?? string.Empty).ToLower().Contains(search));
        }

        versions = versions.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            versions = versions.OrderByDescending(x => x.VersionNumber).ThenByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectVersions(versions).ToPagedAsync(query));
    }

    [HttpPost("{documentId:guid}/versions")]
    [HasPermission("DocumentVersions.Create")]
    public async Task<ActionResult<DocumentVersionDto>> AddVersion(Guid documentId, AddDocumentVersionRequestDto dto)
    {
        var document = await _dbContext.Documents.FirstOrDefaultAsync(x => x.Id == documentId);
        if (document is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.FileName))
        {
            return BadRequest("File name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.ContentType))
        {
            return BadRequest("Content type is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.StoragePath))
        {
            return BadRequest("Storage path is required.");
        }

        var latestVersion = await _dbContext.DocumentVersions
            .Where(x => x.DocumentId == documentId)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync() ?? 0;

        var versionNumber = dto.VersionNumber ?? (latestVersion + 1);
        if (versionNumber <= 0)
        {
            return BadRequest("Version number must be greater than zero.");
        }

        if (await _dbContext.DocumentVersions.AnyAsync(x => x.DocumentId == documentId && x.VersionNumber == versionNumber))
        {
            return BadRequest("Version number already exists for this document.");
        }

        var version = new DocumentVersion
        {
            DocumentId = documentId,
            VersionNumber = versionNumber,
            FileName = dto.FileName.Trim(),
            ContentType = dto.ContentType.Trim(),
            FileSizeBytes = dto.FileSizeBytes,
            StoragePath = dto.StoragePath.Trim(),
            ChangeSummary = TrimToNull(dto.ChangeSummary),
        };

        _dbContext.DocumentVersions.Add(version);

        document.FileName = version.FileName;
        document.ContentType = version.ContentType;
        document.FileSizeBytes = version.FileSizeBytes;
        document.StoragePath = version.StoragePath;
        document.CurrentVersion = Math.Max(document.CurrentVersion, versionNumber);

        await _dbContext.SaveChangesAsync();

        var created = await ProjectVersions(_dbContext.DocumentVersions.Where(x => x.Id == version.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Document version was created but could not be loaded.") : Ok(created);
    }

    [HttpGet("{id:guid}/download")]
    [HasPermission("Documents.Download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var document = await _dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (document is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            document.Id,
            document.DocumentNumber,
            document.FileName,
            document.ContentType,
            document.FileSizeBytes,
            document.StoragePath,
            Message = "Document download is scaffolded. Integrate physical file storage provider in a follow-up increment.",
        });
    }

    private async Task<string?> ValidateDocumentAsync(Guid? documentId, UpsertDocumentRequestDto dto)
    {
        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.DocumentCategoryId && x.LookupCategory.Code == DocumentCategoryCode))
        {
            return "Document category is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.DocumentStatusId && x.LookupCategory.Code == DocumentStatusCode))
        {
            return "Document status is invalid.";
        }

        if (dto.AccountId.HasValue && !await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId.Value))
        {
            return "Account is invalid.";
        }

        if (dto.ContactId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value))
        {
            return "Contact is invalid.";
        }

        if (dto.LeadId.HasValue && !await _dbContext.Leads.AnyAsync(x => x.Id == dto.LeadId.Value))
        {
            return "Lead is invalid.";
        }

        if (dto.OpportunityId.HasValue && !await _dbContext.Opportunities.AnyAsync(x => x.Id == dto.OpportunityId.Value))
        {
            return "Opportunity is invalid.";
        }

        if (dto.CaseId.HasValue && !await _dbContext.ServiceCases.AnyAsync(x => x.Id == dto.CaseId.Value))
        {
            return "Case is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value && !x.IsDeleted))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.FileName))
        {
            return "File name is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.ContentType))
        {
            return "Content type is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.StoragePath))
        {
            return "Storage path is required.";
        }

        if (dto.EffectiveDate.HasValue && dto.ExpiryDate.HasValue && dto.ExpiryDate.Value < dto.EffectiveDate.Value)
        {
            return "Expiry date must be greater than or equal to effective date.";
        }

        if (!string.IsNullOrWhiteSpace(dto.DocumentNumber))
        {
            var number = dto.DocumentNumber.Trim();
            var exists = await _dbContext.Documents.AnyAsync(x => x.Id != documentId && x.DocumentNumber == number);
            if (exists)
            {
                return "Document number already exists.";
            }
        }

        return null;
    }

    private void ApplyDocumentValues(Document document, UpsertDocumentRequestDto dto, bool isCreate)
    {
        document.DocumentNumber = TrimToNull(dto.DocumentNumber) ?? string.Empty;
        document.Title = dto.Title.Trim();
        document.Description = TrimToNull(dto.Description);
        document.FileName = dto.FileName.Trim();
        document.ContentType = dto.ContentType.Trim();
        document.FileSizeBytes = dto.FileSizeBytes;
        document.StoragePath = dto.StoragePath.Trim();
        document.DocumentCategoryId = dto.DocumentCategoryId;
        document.DocumentStatusId = dto.DocumentStatusId;
        document.AccountId = dto.AccountId;
        document.ContactId = dto.ContactId;
        document.LeadId = dto.LeadId;
        document.OpportunityId = dto.OpportunityId;
        document.CaseId = dto.CaseId;
        document.EffectiveDate = dto.EffectiveDate;
        document.ExpiryDate = dto.ExpiryDate;
        document.IsConfidential = dto.IsConfidential;
        document.CurrentVersion = dto.CurrentVersion ?? (isCreate ? 1 : document.CurrentVersion);
        document.IsActive = dto.IsActive;
        document.OwnerUserId = dto.OwnerUserId;
        document.OwnerTeamId = dto.OwnerTeamId;
    }

    private async Task<List<LookupOptionDto>> GetLookupOptionsAsync(string categoryCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
            })
            .ToListAsync();
    }

    private static IQueryable<DocumentDto> ProjectDocuments(IQueryable<Document> query)
    {
        return query.Select(x => new DocumentDto
        {
            Id = x.Id,
            DocumentNumber = x.DocumentNumber,
            Title = x.Title,
            Description = x.Description,
            FileName = x.FileName,
            ContentType = x.ContentType,
            FileSizeBytes = x.FileSizeBytes,
            StoragePath = x.StoragePath,
            DocumentCategoryId = x.DocumentCategoryId,
            DocumentCategoryName = x.DocumentCategory.Name,
            DocumentCategoryCode = x.DocumentCategory.Code,
            DocumentStatusId = x.DocumentStatusId,
            DocumentStatusName = x.DocumentStatus.Name,
            DocumentStatusCode = x.DocumentStatus.Code,
            AccountId = x.AccountId,
            AccountName = x.Account != null ? x.Account.Name : null,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            LeadId = x.LeadId,
            LeadTopic = x.Lead != null ? x.Lead.Topic : null,
            OpportunityId = x.OpportunityId,
            OpportunityTopic = x.Opportunity != null ? x.Opportunity.Topic : null,
            CaseId = x.CaseId,
            CaseNumber = x.Case != null ? x.Case.CaseNumber : null,
            EffectiveDate = x.EffectiveDate,
            ExpiryDate = x.ExpiryDate,
            IsConfidential = x.IsConfidential,
            CurrentVersion = x.CurrentVersion,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static IQueryable<DocumentVersionDto> ProjectVersions(IQueryable<DocumentVersion> query)
    {
        return query.Select(x => new DocumentVersionDto
        {
            Id = x.Id,
            DocumentId = x.DocumentId,
            VersionNumber = x.VersionNumber,
            FileName = x.FileName,
            ContentType = x.ContentType,
            FileSizeBytes = x.FileSizeBytes,
            StoragePath = x.StoragePath,
            ChangeSummary = x.ChangeSummary,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
