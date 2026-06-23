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
[Route("api/cases")]
public class CasesController : ControllerBase
{
    private const string CaseStatusCategoryCode = "CASE_STATUS";
    private const string CasePriorityCategoryCode = "CASE_PRIORITY";
    private const string CaseSeverityCategoryCode = "CASE_SEVERITY";
    private const string CaseCategoryCategoryCode = "CASE_CATEGORY";
    private const string CaseSourceCategoryCode = "CASE_SOURCE";

    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly ICurrentUserContext _currentUserContext;

    public CasesController(
        AppDbContext dbContext,
        INumberSequenceService numberSequenceService,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [HasPermission("Cases.View")]
    public async Task<ActionResult<PagedResult<CaseDto>>> GetCases([FromQuery] CaseFilterDto query)
    {
        var cases = _dbContext.ServiceCases.AsQueryable();

        if (query.AccountId.HasValue)
        {
            cases = cases.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.CaseStatusId.HasValue)
        {
            cases = cases.Where(x => x.CaseStatusId == query.CaseStatusId.Value);
        }

        if (query.PriorityId.HasValue)
        {
            cases = cases.Where(x => x.PriorityId == query.PriorityId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            cases = cases.Where(x => x.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.IsActive.HasValue)
        {
            cases = cases.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            cases = cases.Where(x =>
                x.CaseNumber.ToLower().Contains(search) ||
                x.Subject.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(search)));
        }

        cases = cases.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            cases = cases.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectCases(cases).ToPagedAsync(query));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Cases.View")]
    public async Task<ActionResult<CaseDto>> GetCase(Guid id)
    {
        var serviceCase = await ProjectCases(_dbContext.ServiceCases.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return serviceCase is null ? NotFound() : Ok(serviceCase);
    }

    [HttpPost]
    [HasPermission("Cases.Create")]
    public async Task<ActionResult<CaseDto>> CreateCase(UpsertCaseRequestDto dto)
    {
        var validationError = await ValidateCaseAsync(null, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var serviceCase = new ServiceCase();
        ApplyCaseValues(serviceCase, dto, isCreate: true);

        if (string.IsNullOrWhiteSpace(serviceCase.CaseNumber))
        {
            serviceCase.CaseNumber = await _numberSequenceService.GenerateNextAsync("CASE");
        }

        if (await _dbContext.ServiceCases.AnyAsync(x => x.CaseNumber == serviceCase.CaseNumber))
        {
            return BadRequest("Case number already exists.");
        }

        _dbContext.ServiceCases.Add(serviceCase);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectCases(_dbContext.ServiceCases.Where(x => x.Id == serviceCase.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Case was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Cases.Update")]
    public async Task<IActionResult> UpdateCase(Guid id, UpsertCaseRequestDto dto)
    {
        var serviceCase = await _dbContext.ServiceCases.FirstOrDefaultAsync(x => x.Id == id);
        if (serviceCase is null)
        {
            return NotFound();
        }

        var validationError = await ValidateCaseAsync(id, dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyCaseValues(serviceCase, dto, isCreate: false);
        if (string.IsNullOrWhiteSpace(serviceCase.CaseNumber))
        {
            serviceCase.CaseNumber = await _numberSequenceService.GenerateNextAsync("CASE");
        }

        if (await _dbContext.ServiceCases.AnyAsync(x => x.Id != id && x.CaseNumber == serviceCase.CaseNumber))
        {
            return BadRequest("Case number already exists.");
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Cases.Delete")]
    public async Task<IActionResult> DeleteCase(Guid id)
    {
        var serviceCase = await _dbContext.ServiceCases.FirstOrDefaultAsync(x => x.Id == id);
        if (serviceCase is null)
        {
            return NotFound();
        }

        serviceCase.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("lookup")]
    [HasPermission("Cases.View")]
    public async Task<ActionResult<object>> GetLookup()
    {
        return Ok(new
        {
            CaseStatuses = await GetLookupOptionsAsync(CaseStatusCategoryCode),
            Priorities = await GetLookupOptionsAsync(CasePriorityCategoryCode),
            Severities = await GetLookupOptionsAsync(CaseSeverityCategoryCode),
            Categories = await GetLookupOptionsAsync(CaseCategoryCategoryCode),
            Sources = await GetLookupOptionsAsync(CaseSourceCategoryCode)
        });
    }

    [HttpGet("{caseId:guid}/comments")]
    [HasPermission("CaseComments.View")]
    public async Task<ActionResult<PagedResult<CaseCommentDto>>> GetComments(Guid caseId, [FromQuery] CaseCommentFilterDto query)
    {
        if (!await _dbContext.ServiceCases.AnyAsync(x => x.Id == caseId))
        {
            return NotFound();
        }

        query.CaseId = caseId;
        var comments = _dbContext.CaseComments.AsQueryable();

        if (query.CaseId.HasValue)
        {
            comments = comments.Where(x => x.CaseId == query.CaseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            comments = comments.Where(x => x.CommentText.ToLower().Contains(search));
        }

        comments = comments.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            comments = comments.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectComments(comments).ToPagedAsync(query));
    }

    [HttpPost("{caseId:guid}/comments")]
    [HasPermission("CaseComments.Create")]
    public async Task<ActionResult<CaseCommentDto>> AddComment(Guid caseId, AddCaseCommentRequestDto dto)
    {
        if (!await _dbContext.ServiceCases.AnyAsync(x => x.Id == caseId))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.CommentText))
        {
            return BadRequest("Comment text is required.");
        }

        var comment = new CaseComment
        {
            CaseId = caseId,
            CommentText = dto.CommentText.Trim(),
            IsInternal = dto.IsInternal
        };

        _dbContext.CaseComments.Add(comment);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectComments(_dbContext.CaseComments.Where(x => x.Id == comment.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Comment was created but could not be loaded.") : Ok(created);
    }

    [HttpDelete("comments/{id:guid}")]
    [HasPermission("CaseComments.Delete")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await _dbContext.CaseComments.FirstOrDefaultAsync(x => x.Id == id);
        if (comment is null)
        {
            return NotFound();
        }

        comment.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/resolve")]
    [HasPermission("Cases.Resolve")]
    public async Task<IActionResult> ResolveCase(Guid id, ResolveCaseRequestDto dto)
    {
        var serviceCase = await _dbContext.ServiceCases.FirstOrDefaultAsync(x => x.Id == id);
        if (serviceCase is null)
        {
            return NotFound();
        }

        var statusCode = await GetCaseStatusCodeAsync(serviceCase.CaseStatusId);
        if (statusCode is "CLOSED")
        {
            return BadRequest("Closed cases must be reopened before resolving.");
        }

        var resolvedStatusId = await GetCaseStatusIdByCodeAsync("RESOLVED");
        if (!resolvedStatusId.HasValue)
        {
            return BadRequest("Resolved status lookup is missing.");
        }

        serviceCase.CaseStatusId = resolvedStatusId.Value;
        serviceCase.ResolvedAt = dto.ResolvedAt ?? DateTime.UtcNow;
        serviceCase.ClosedAt = null;
        if (!string.IsNullOrWhiteSpace(dto.ResolutionSummary))
        {
            serviceCase.ResolutionSummary = dto.ResolutionSummary.Trim();
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    [HasPermission("Cases.Close")]
    public async Task<IActionResult> CloseCase(Guid id, CloseCaseRequestDto dto)
    {
        var serviceCase = await _dbContext.ServiceCases.FirstOrDefaultAsync(x => x.Id == id);
        if (serviceCase is null)
        {
            return NotFound();
        }

        var statusCode = await GetCaseStatusCodeAsync(serviceCase.CaseStatusId);
        if (statusCode is not "RESOLVED")
        {
            return BadRequest("Only resolved cases can be closed.");
        }

        var closedStatusId = await GetCaseStatusIdByCodeAsync("CLOSED");
        if (!closedStatusId.HasValue)
        {
            return BadRequest("Closed status lookup is missing.");
        }

        serviceCase.CaseStatusId = closedStatusId.Value;
        serviceCase.ClosedAt = dto.ClosedAt ?? DateTime.UtcNow;
        if (!serviceCase.ResolvedAt.HasValue)
        {
            serviceCase.ResolvedAt = serviceCase.ClosedAt;
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/reopen")]
    [HasPermission("Cases.Reopen")]
    public async Task<IActionResult> ReopenCase(Guid id, ReopenCaseRequestDto dto)
    {
        var serviceCase = await _dbContext.ServiceCases.FirstOrDefaultAsync(x => x.Id == id);
        if (serviceCase is null)
        {
            return NotFound();
        }

        var statusCode = await GetCaseStatusCodeAsync(serviceCase.CaseStatusId);
        if (statusCode is not ("RESOLVED" or "CLOSED"))
        {
            return BadRequest("Only resolved or closed cases can be reopened.");
        }

        var openStatusId = await GetCaseStatusIdByCodeAsync("OPEN");
        if (!openStatusId.HasValue)
        {
            return BadRequest("Open status lookup is missing.");
        }

        serviceCase.CaseStatusId = openStatusId.Value;
        serviceCase.ResolvedAt = null;
        serviceCase.ClosedAt = null;
        serviceCase.OpenedAt = dto.ReopenedAt ?? DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateCaseAsync(Guid? caseId, UpsertCaseRequestDto dto)
    {
        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return "Account is invalid.";
        }

        if (dto.ContactId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value && x.AccountId == dto.AccountId))
        {
            return "Contact is invalid for the selected account.";
        }

        if (dto.OpportunityId.HasValue && !await _dbContext.Opportunities.AnyAsync(x => x.Id == dto.OpportunityId.Value && x.AccountId == dto.AccountId))
        {
            return "Opportunity is invalid for the selected account.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.CaseStatusId && x.LookupCategory.Code == CaseStatusCategoryCode))
        {
            return "Case status is invalid.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.PriorityId && x.LookupCategory.Code == CasePriorityCategoryCode))
        {
            return "Priority is invalid.";
        }

        if (dto.SeverityId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.SeverityId.Value && x.LookupCategory.Code == CaseSeverityCategoryCode))
        {
            return "Severity is invalid.";
        }

        if (dto.CategoryId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.CategoryId.Value && x.LookupCategory.Code == CaseCategoryCategoryCode))
        {
            return "Category is invalid.";
        }

        if (dto.SourceId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.SourceId.Value && x.LookupCategory.Code == CaseSourceCategoryCode))
        {
            return "Source is invalid.";
        }

        if (dto.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value))
        {
            return "Assigned user is invalid.";
        }

        if (dto.EscalatedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.EscalatedToUserId.Value))
        {
            return "Escalated user is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (string.IsNullOrWhiteSpace(dto.Subject))
        {
            return "Subject is required.";
        }

        var openedAt = dto.OpenedAt ?? DateTime.UtcNow;
        if (dto.DueAt.HasValue && dto.DueAt.Value < openedAt)
        {
            return "Due date must be greater than or equal to opened date.";
        }

        if (dto.ResolvedAt.HasValue && dto.ResolvedAt.Value < openedAt)
        {
            return "Resolved date must be greater than or equal to opened date.";
        }

        if (dto.ClosedAt.HasValue && dto.ResolvedAt.HasValue && dto.ClosedAt.Value < dto.ResolvedAt.Value)
        {
            return "Closed date must be greater than or equal to resolved date.";
        }

        if (!string.IsNullOrWhiteSpace(dto.CaseNumber))
        {
            var number = dto.CaseNumber.Trim();
            var exists = await _dbContext.ServiceCases.AnyAsync(x => x.Id != caseId && x.CaseNumber == number);
            if (exists)
            {
                return "Case number already exists.";
            }
        }

        return null;
    }

    private void ApplyCaseValues(ServiceCase serviceCase, UpsertCaseRequestDto dto, bool isCreate)
    {
        serviceCase.CaseNumber = TrimToNull(dto.CaseNumber) ?? string.Empty;
        serviceCase.AccountId = dto.AccountId;
        serviceCase.ContactId = dto.ContactId;
        serviceCase.OpportunityId = dto.OpportunityId;
        serviceCase.Subject = dto.Subject.Trim();
        serviceCase.Description = TrimToNull(dto.Description);
        serviceCase.CaseStatusId = dto.CaseStatusId;
        serviceCase.PriorityId = dto.PriorityId;
        serviceCase.SeverityId = dto.SeverityId;
        serviceCase.CategoryId = dto.CategoryId;
        serviceCase.SourceId = dto.SourceId;
        serviceCase.AssignedToUserId = dto.AssignedToUserId;
        serviceCase.EscalatedToUserId = dto.EscalatedToUserId;
        serviceCase.OpenedAt = dto.OpenedAt ?? (isCreate ? DateTime.UtcNow : serviceCase.OpenedAt);
        serviceCase.DueAt = dto.DueAt;
        serviceCase.ResolvedAt = dto.ResolvedAt;
        serviceCase.ClosedAt = dto.ClosedAt;
        serviceCase.ResolutionSummary = TrimToNull(dto.ResolutionSummary);
        serviceCase.IsActive = dto.IsActive;
        serviceCase.OwnerUserId = dto.OwnerUserId;
        serviceCase.OwnerTeamId = dto.OwnerTeamId;
    }

    private async Task<Guid?> GetCaseStatusIdByCodeAsync(string code)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == CaseStatusCategoryCode && x.Code == code)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetCaseStatusCodeAsync(Guid statusId)
    {
        return await _dbContext.LookupValues
            .Where(x => x.Id == statusId)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();
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
                Code = x.Code
            })
            .ToListAsync();
    }

    private static IQueryable<CaseDto> ProjectCases(IQueryable<ServiceCase> query)
    {
        return query.Select(x => new CaseDto
        {
            Id = x.Id,
            CaseNumber = x.CaseNumber,
            AccountId = x.AccountId,
            AccountName = x.Account.Name,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            OpportunityId = x.OpportunityId,
            OpportunityTopic = x.Opportunity != null ? x.Opportunity.Topic : null,
            Subject = x.Subject,
            Description = x.Description,
            CaseStatusId = x.CaseStatusId,
            CaseStatusName = x.CaseStatus.Name,
            CaseStatusCode = x.CaseStatus.Code,
            PriorityId = x.PriorityId,
            PriorityName = x.Priority.Name,
            SeverityId = x.SeverityId,
            SeverityName = x.Severity != null ? x.Severity.Name : null,
            CategoryId = x.CategoryId,
            CategoryName = x.Category != null ? x.Category.Name : null,
            SourceId = x.SourceId,
            SourceName = x.Source != null ? x.Source.Name : null,
            AssignedToUserId = x.AssignedToUserId,
            AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName : null,
            EscalatedToUserId = x.EscalatedToUserId,
            EscalatedToUserName = x.EscalatedToUser != null ? x.EscalatedToUser.FirstName + " " + x.EscalatedToUser.LastName : null,
            OpenedAt = x.OpenedAt,
            DueAt = x.DueAt,
            ResolvedAt = x.ResolvedAt,
            ClosedAt = x.ClosedAt,
            ResolutionSummary = x.ResolutionSummary,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerTeamId = x.OwnerTeamId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static IQueryable<CaseCommentDto> ProjectComments(IQueryable<CaseComment> query)
    {
        return query.Select(x => new CaseCommentDto
        {
            Id = x.Id,
            CaseId = x.CaseId,
            CommentText = x.CommentText,
            IsInternal = x.IsInternal,
            CreatedAt = x.CreatedAt,
            CreatedById = x.CreatedById,
            CreatedByName = x.CreatedById.HasValue ?
                x.Case.AssignedToUser != null && x.Case.AssignedToUser.Id == x.CreatedById.Value
                    ? x.Case.AssignedToUser.FirstName + " " + x.Case.AssignedToUser.LastName
                    : x.Case.OwnerUser != null && x.Case.OwnerUser.Id == x.CreatedById.Value
                        ? x.Case.OwnerUser.FirstName + " " + x.Case.OwnerUser.LastName
                        : null
                : null
        });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
