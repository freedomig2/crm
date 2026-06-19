using System.Text.Json;
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
[Route("api/leads")]
public class LeadsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILeadScoringService _leadScoringService;
    private readonly ILeadConversionService _leadConversionService;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly ICurrentUserContext _currentUserContext;

    public LeadsController(
        AppDbContext dbContext,
        ILeadScoringService leadScoringService,
        ILeadConversionService leadConversionService,
        INumberSequenceService numberSequenceService,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _leadScoringService = leadScoringService;
        _leadConversionService = leadConversionService;
        _numberSequenceService = numberSequenceService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [HasPermission("Leads.View")]
    public async Task<ActionResult<PagedResult<LeadDto>>> GetLeads([FromQuery] LeadFilterDto query)
    {
        var leadsQuery = _dbContext.Leads.AsQueryable();

        if (query.LeadSourceId.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.LeadSourceId == query.LeadSourceId.Value);
        }

        if (query.LeadStatusId.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.LeadStatusId == query.LeadStatusId.Value);
        }

        if (query.QualificationStatusId.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.QualificationStatusId == query.QualificationStatusId.Value);
        }

        if (query.RatingId.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.RatingId == query.RatingId.Value);
        }

        if (query.OwnerUserId.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.OwnerUserId == query.OwnerUserId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.IsConverted.HasValue)
        {
            leadsQuery = query.IsConverted.Value
                ? leadsQuery.Where(x => x.ConvertedAt != null)
                : leadsQuery.Where(x => x.ConvertedAt == null);
        }

        if (query.IsActive.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            leadsQuery = leadsQuery.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            leadsQuery = leadsQuery.Where(x =>
                x.LeadNumber.ToLower().Contains(search) ||
                x.Topic.ToLower().Contains(search) ||
                (x.FullName ?? string.Empty).ToLower().Contains(search) ||
                (x.CompanyName ?? string.Empty).ToLower().Contains(search) ||
                (x.Email ?? string.Empty).ToLower().Contains(search) ||
                (x.MobilePhone ?? string.Empty).ToLower().Contains(search));
        }

        leadsQuery = leadsQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            leadsQuery = leadsQuery.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectLeads(leadsQuery).ToPagedAsync(query));
    }

    [HttpGet("dashboard-summary")]
    [HasPermission("Leads.View")]
    public async Task<ActionResult<LeadDashboardSummaryDto>> GetDashboardSummary()
    {
        var totalLeads = await _dbContext.Leads.CountAsync();
        var averageLeadScore = totalLeads == 0 ? 0 : await _dbContext.Leads.AverageAsync(x => (double)x.Score);

        var summary = new LeadDashboardSummaryDto
        {
            TotalLeads = totalLeads,
            NewLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "NEW"),
            QualifiedLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "QUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "QUALIFIED")),
            ConvertedLeads = await _dbContext.Leads.CountAsync(x => x.ConvertedAt != null || x.LeadStatus.Code == "CONVERTED"),
            DisqualifiedLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "DISQUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "DISQUALIFIED")),
            AverageLeadScore = Math.Round(averageLeadScore, 1),
            HotLeads = await _dbContext.Leads.CountAsync(x => x.ScoreGrade == "Hot"),
            LeadsBySource = await _dbContext.Leads
                .GroupBy(x => x.LeadSource != null ? x.LeadSource.Name : "Not set")
                .Select(x => new LeadDashboardGroupDto { Name = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            LeadsByStatus = await _dbContext.Leads
                .GroupBy(x => x.LeadStatus.Name)
                .Select(x => new LeadDashboardGroupDto { Name = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            RecentLeads = await _dbContext.Leads
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new LeadDashboardItemDto
                {
                    Id = x.Id,
                    LeadNumber = x.LeadNumber,
                    Topic = x.Topic,
                    StatusName = x.LeadStatus.Name,
                    Score = x.Score,
                    CreatedAt = x.CreatedAt,
                    ConvertedAt = x.ConvertedAt
                })
                .ToListAsync(),
            RecentlyConvertedLeads = await _dbContext.Leads
                .Where(x => x.ConvertedAt != null)
                .OrderByDescending(x => x.ConvertedAt)
                .Take(5)
                .Select(x => new LeadDashboardItemDto
                {
                    Id = x.Id,
                    LeadNumber = x.LeadNumber,
                    Topic = x.Topic,
                    StatusName = x.LeadStatus.Name,
                    Score = x.Score,
                    CreatedAt = x.CreatedAt,
                    ConvertedAt = x.ConvertedAt
                })
                .ToListAsync()
        };

        return Ok(summary);
    }

    [HttpGet("lookup")]
    [HasPermission("Leads.View")]
    public async Task<ActionResult<LeadLookupDto>> GetLookup()
    {
        return Ok(new LeadLookupDto
        {
            LeadSources = await GetLookupOptionsAsync("LEAD_SOURCE"),
            LeadStatuses = await GetLookupOptionsAsync("LEAD_STATUS"),
            QualificationStatuses = await GetLookupOptionsAsync("LEAD_QUALIFICATION_STATUS"),
            Ratings = await GetLookupOptionsAsync("LEAD_RATING"),
            Industries = await GetLookupOptionsAsync("INDUSTRY"),
            DisqualificationReasons = await GetLookupOptionsAsync("LEAD_DISQUALIFICATION_REASON"),
            ActivityTypes = await GetLookupOptionsAsync("ACTIVITY_TYPE"),
            ActivityStatuses = await GetLookupOptionsAsync("ACTIVITY_STATUS"),
            Priorities = await GetLookupOptionsAsync("PRIORITY"),
            ScoreRuleTypes = await GetLookupOptionsAsync("LEAD_SCORE_RULE_TYPE")
        });
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Leads.View")]
    public async Task<ActionResult<LeadDto>> GetLead(Guid id)
    {
        var lead = await GetLeadDtoAsync(id);
        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpPost]
    [HasPermission("Leads.Create")]
    public async Task<ActionResult<LeadDto>> CreateLead(UpsertLeadRequestDto dto)
    {
        var statusId = await ResolveLeadStatusIdAsync(dto.LeadStatusId, "NEW");
        if (!statusId.HasValue)
        {
            return BadRequest("Lead status is required.");
        }

        var validationError = await ValidateReferencesAsync(dto, statusId.Value);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            LeadNumber = await _numberSequenceService.GenerateNextAsync("LEAD"),
            LeadStatusId = statusId.Value
        };

        ApplyLeadValues(lead, dto, statusId.Value);
        _dbContext.Leads.Add(lead);
        await _leadScoringService.ApplyScoreAsync(lead);
        await _dbContext.SaveChangesAsync();

        var created = await GetLeadDtoAsync(lead.Id);
        return created is null ? Problem("Lead was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Leads.Update")]
    public async Task<IActionResult> UpdateLead(Guid id, UpsertLeadRequestDto dto)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        var statusId = await ResolveLeadStatusIdAsync(dto.LeadStatusId, "NEW");
        if (!statusId.HasValue)
        {
            return BadRequest("Lead status is required.");
        }

        var validationError = await ValidateReferencesAsync(dto, statusId.Value);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyLeadValues(lead, dto, statusId.Value);
        if (string.IsNullOrWhiteSpace(lead.LeadNumber))
        {
            lead.LeadNumber = await _numberSequenceService.GenerateNextAsync("LEAD");
        }

        await _leadScoringService.ApplyScoreAsync(lead);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Leads.Delete")]
    public async Task<IActionResult> DeleteLead(Guid id)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        lead.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/assign")]
    [HasPermission("Leads.Assign")]
    public async Task<IActionResult> AssignLead(Guid id, LeadAssignRequestDto dto)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        if (dto.AssignedToUserId.HasValue == dto.AssignedToTeamId.HasValue)
        {
            return BadRequest("Assign the lead to either a user or a team.");
        }

        if (dto.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value && !x.IsDeleted))
        {
            return BadRequest("Assigned user could not be found.");
        }

        if (dto.AssignedToTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.AssignedToTeamId.Value))
        {
            return BadRequest("Assigned team could not be found.");
        }

        lead.AssignedToUserId = dto.AssignedToUserId;
        lead.AssignedToTeamId = dto.AssignedToTeamId;
        lead.OwnerUserId = dto.AssignedToUserId;
        lead.OwnerTeamId = dto.AssignedToTeamId;
        AddLeadActionAudit(lead.Id, "Assign", new { dto.AssignedToUserId, dto.AssignedToTeamId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/qualify")]
    [HasPermission("Leads.Qualify")]
    public async Task<IActionResult> QualifyLead(Guid id)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        var leadStatusId = await GetLookupValueIdAsync("LEAD_STATUS", "QUALIFIED");
        var qualificationStatusId = await GetLookupValueIdAsync("LEAD_QUALIFICATION_STATUS", "QUALIFIED");
        if (!leadStatusId.HasValue || !qualificationStatusId.HasValue)
        {
            return BadRequest("Lead qualification lookup values are not configured.");
        }

        lead.LeadStatusId = leadStatusId.Value;
        lead.QualificationStatusId = qualificationStatusId.Value;
        lead.DisqualifiedReasonId = null;
        AddLeadActionAudit(lead.Id, "Qualify", new { leadStatusId, qualificationStatusId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/disqualify")]
    [HasPermission("Leads.Disqualify")]
    public async Task<IActionResult> DisqualifyLead(Guid id, LeadDisqualifyRequestDto dto)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.DisqualifiedReasonId && x.LookupCategory.Code == "LEAD_DISQUALIFICATION_REASON"))
        {
            return BadRequest("Disqualification reason is required.");
        }

        var leadStatusId = await GetLookupValueIdAsync("LEAD_STATUS", "DISQUALIFIED");
        var qualificationStatusId = await GetLookupValueIdAsync("LEAD_QUALIFICATION_STATUS", "DISQUALIFIED");
        if (!leadStatusId.HasValue || !qualificationStatusId.HasValue)
        {
            return BadRequest("Lead disqualification lookup values are not configured.");
        }

        lead.LeadStatusId = leadStatusId.Value;
        lead.QualificationStatusId = qualificationStatusId.Value;
        lead.DisqualifiedReasonId = dto.DisqualifiedReasonId;
        AddLeadActionAudit(lead.Id, "Disqualify", new { dto.DisqualifiedReasonId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/convert")]
    [HasPermission("Leads.Convert")]
    public async Task<ActionResult<LeadConversionResultDto>> ConvertLead(Guid id, LeadConversionRequestDto dto)
    {
        try
        {
            var result = await _leadConversionService.ConvertAsync(id, dto);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/calculate-score")]
    [HasPermission("Leads.Score")]
    public async Task<IActionResult> CalculateScore(Guid id)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        await _leadScoringService.ApplyScoreAsync(lead);
        AddLeadActionAudit(lead.Id, "ScoreCalculated", new { lead.Score, lead.ScoreGrade });
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}/timeline")]
    [HasPermission("Leads.ViewTimeline")]
    public async Task<ActionResult<IReadOnlyCollection<LeadTimelineItemDto>>> GetTimeline(Guid id)
    {
        if (!await _dbContext.Leads.AnyAsync(x => x.Id == id))
        {
            return NotFound();
        }

        var activities = await _dbContext.LeadActivities
            .Where(x => x.LeadId == id)
            .Select(x => new LeadTimelineItemDto
            {
                Id = x.Id,
                ItemType = "Activity",
                Title = x.Subject,
                Description = x.Description,
                OccurredAt = x.CompletedDate ?? x.ActivityDate,
                Status = x.Status.Name,
                Priority = x.Priority != null ? x.Priority.Name : null,
                AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.Email : null
            })
            .ToListAsync();

        var auditEvents = await _dbContext.AuditLogs
            .Where(x => x.EntityName == nameof(Lead) && x.EntityId == id.ToString())
            .Select(x => new LeadTimelineItemDto
            {
                Id = x.Id,
                ItemType = "Audit",
                Title = x.Action,
                Description = x.NewValues,
                OccurredAt = x.CreatedAt,
                Status = null,
                Priority = null,
                AssignedToName = null
            })
            .ToListAsync();

        return Ok(activities.Concat(auditEvents).OrderByDescending(x => x.OccurredAt).ToList());
    }

    private async Task<string?> ValidateReferencesAsync(UpsertLeadRequestDto dto, Guid resolvedStatusId)
    {
        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == resolvedStatusId && x.LookupCategory.Code == "LEAD_STATUS"))
        {
            return "Lead status is invalid.";
        }

        var lookupChecks = new (Guid? Id, string Category, string Label)[]
        {
            (dto.LeadSourceId, "LEAD_SOURCE", "Lead source"),
            (dto.QualificationStatusId, "LEAD_QUALIFICATION_STATUS", "Qualification status"),
            (dto.RatingId, "LEAD_RATING", "Rating"),
            (dto.IndustryId, "INDUSTRY", "Industry"),
            (dto.DisqualifiedReasonId, "LEAD_DISQUALIFICATION_REASON", "Disqualification reason")
        };

        foreach (var (lookupId, category, label) in lookupChecks)
        {
            if (lookupId.HasValue && !await _dbContext.LookupValues.AnyAsync(x => x.Id == lookupId.Value && x.LookupCategory.Code == category))
            {
                return $"{label} is invalid.";
            }
        }

        if (dto.AssignedToUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.AssignedToUserId.Value && !x.IsDeleted))
        {
            return "Assigned user is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value && !x.IsDeleted))
        {
            return "Owner user is invalid.";
        }

        if (dto.AssignedToTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.AssignedToTeamId.Value))
        {
            return "Assigned team is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        return null;
    }

    private async Task<Guid?> ResolveLeadStatusIdAsync(Guid dtoStatusId, string defaultCode)
    {
        if (dtoStatusId != Guid.Empty)
        {
            return dtoStatusId;
        }

        return await GetLookupValueIdAsync("LEAD_STATUS", defaultCode);
    }

    private async Task<Guid?> GetLookupValueIdAsync(string categoryCode, string valueCode)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.Code == valueCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<IReadOnlyCollection<LookupOptionDto>> GetLookupOptionsAsync(string categoryCode)
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

    private void AddLeadActionAudit(Guid leadId, string action, object values)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = nameof(Lead),
            EntityId = leadId.ToString(),
            Action = action,
            NewValues = JsonSerializer.Serialize(values),
            UserId = _currentUserContext.UserId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = _currentUserContext.UserId
        });
    }

    private static void ApplyLeadValues(Lead lead, UpsertLeadRequestDto dto, Guid leadStatusId)
    {
        lead.Topic = dto.Topic.Trim();
        lead.FirstName = TrimToNull(dto.FirstName);
        lead.MiddleName = TrimToNull(dto.MiddleName);
        lead.LastName = TrimToNull(dto.LastName);
        lead.FullName = BuildFullName(lead.FirstName, lead.MiddleName, lead.LastName);
        lead.CompanyName = TrimToNull(dto.CompanyName);
        lead.JobTitle = TrimToNull(dto.JobTitle);
        lead.Email = TrimToNull(dto.Email);
        lead.MobilePhone = TrimToNull(dto.MobilePhone);
        lead.WorkPhone = TrimToNull(dto.WorkPhone);
        lead.Website = TrimToNull(dto.Website);
        lead.LeadSourceId = dto.LeadSourceId;
        lead.LeadStatusId = leadStatusId;
        lead.QualificationStatusId = dto.QualificationStatusId;
        lead.RatingId = dto.RatingId;
        lead.IndustryId = dto.IndustryId;
        lead.EstimatedValue = dto.EstimatedValue;
        lead.EstimatedCloseDate = dto.EstimatedCloseDate;
        lead.AssignedToUserId = dto.AssignedToUserId;
        lead.AssignedToTeamId = dto.AssignedToTeamId;
        lead.DisqualifiedReasonId = dto.DisqualifiedReasonId;
        lead.Description = TrimToNull(dto.Description);
        lead.Notes = TrimToNull(dto.Notes);
        lead.OwnerUserId = dto.OwnerUserId;
        lead.OwnerTeamId = dto.OwnerTeamId;
        lead.IsActive = dto.IsActive;
    }

    private async Task<LeadDto?> GetLeadDtoAsync(Guid id)
    {
        return await ProjectLeads(_dbContext.Leads.Where(x => x.Id == id)).FirstOrDefaultAsync();
    }

    private static IQueryable<LeadDto> ProjectLeads(IQueryable<Lead> query)
    {
        return query.Select(x => new LeadDto
        {
            Id = x.Id,
            LeadNumber = x.LeadNumber,
            Topic = x.Topic,
            FirstName = x.FirstName,
            MiddleName = x.MiddleName,
            LastName = x.LastName,
            FullName = x.FullName,
            CompanyName = x.CompanyName,
            JobTitle = x.JobTitle,
            Email = x.Email,
            MobilePhone = x.MobilePhone,
            WorkPhone = x.WorkPhone,
            Website = x.Website,
            LeadSourceId = x.LeadSourceId,
            LeadSourceName = x.LeadSource != null ? x.LeadSource.Name : null,
            LeadStatusId = x.LeadStatusId,
            LeadStatusName = x.LeadStatus.Name,
            QualificationStatusId = x.QualificationStatusId,
            QualificationStatusName = x.QualificationStatus != null ? x.QualificationStatus.Name : null,
            RatingId = x.RatingId,
            RatingName = x.Rating != null ? x.Rating.Name : null,
            IndustryId = x.IndustryId,
            IndustryName = x.Industry != null ? x.Industry.Name : null,
            EstimatedValue = x.EstimatedValue,
            EstimatedCloseDate = x.EstimatedCloseDate,
            Score = x.Score,
            ScoreGrade = x.ScoreGrade,
            AssignedToUserId = x.AssignedToUserId,
            AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.Email : null,
            AssignedToTeamId = x.AssignedToTeamId,
            AssignedToTeamName = x.AssignedToTeam != null ? x.AssignedToTeam.Name : null,
            ConvertedAccountId = x.ConvertedAccountId,
            ConvertedAccountName = x.ConvertedAccount != null ? x.ConvertedAccount.Name : null,
            ConvertedContactId = x.ConvertedContactId,
            ConvertedContactName = x.ConvertedContact != null ? x.ConvertedContact.FullName : null,
            ConvertedOpportunityId = x.ConvertedOpportunityId,
            ConvertedAt = x.ConvertedAt,
            ConvertedById = x.ConvertedById,
            ConvertedByName = x.ConvertedBy != null ? x.ConvertedBy.Email : null,
            DisqualifiedReasonId = x.DisqualifiedReasonId,
            DisqualifiedReasonName = x.DisqualifiedReason != null ? x.DisqualifiedReason.Name : null,
            Description = x.Description,
            Notes = x.Notes,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerUserName = x.OwnerUser != null ? x.OwnerUser.Email : null,
            OwnerTeamId = x.OwnerTeamId,
            OwnerTeamName = x.OwnerTeam != null ? x.OwnerTeam.Name : null,
            CreatedAt = x.CreatedAt
        });
    }

    private static string? BuildFullName(string? firstName, string? middleName, string? lastName)
    {
        var value = string.Join(' ', new[] { firstName, middleName, lastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim()));
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
