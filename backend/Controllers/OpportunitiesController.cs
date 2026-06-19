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
[Route("api/opportunities")]
public class OpportunitiesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly ICurrentUserContext _currentUserContext;

    public OpportunitiesController(
        AppDbContext dbContext,
        INumberSequenceService numberSequenceService,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [HasPermission("Opportunities.View")]
    public async Task<ActionResult<PagedResult<OpportunityDto>>> GetOpportunities([FromQuery] OpportunityFilterDto query)
    {
        var opportunitiesQuery = _dbContext.Opportunities.AsQueryable();

        if (query.AccountId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.AccountId == query.AccountId.Value);
        }

        if (query.OpportunityStageId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.OpportunityStageId == query.OpportunityStageId.Value);
        }

        if (query.OpportunityStatusId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.OpportunityStatusId == query.OpportunityStatusId.Value);
        }

        if (query.RatingId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.RatingId == query.RatingId.Value);
        }

        if (query.PriorityId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.PriorityId == query.PriorityId.Value);
        }

        if (query.SourceId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.SourceId == query.SourceId.Value);
        }

        if (query.OwnerUserId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.OwnerUserId == query.OwnerUserId.Value);
        }

        if (query.OwnerTeamId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.OwnerTeamId == query.OwnerTeamId.Value);
        }

        if (query.EstimatedCloseFrom.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.EstimatedCloseDate >= query.EstimatedCloseFrom.Value);
        }

        if (query.EstimatedCloseTo.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.EstimatedCloseDate <= query.EstimatedCloseTo.Value);
        }

        if (query.MinRevenue.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.EstimatedRevenue >= query.MinRevenue.Value);
        }

        if (query.MaxRevenue.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.EstimatedRevenue <= query.MaxRevenue.Value);
        }

        if (query.IsActive.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            opportunitiesQuery = opportunitiesQuery.Where(x =>
                x.OpportunityNumber.ToLower().Contains(search) ||
                x.Topic.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                (x.Contact != null && x.Contact.FullName.ToLower().Contains(search)) ||
                (x.Lead != null && x.Lead.Topic.ToLower().Contains(search)));
        }

        opportunitiesQuery = opportunitiesQuery.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            opportunitiesQuery = opportunitiesQuery.OrderByDescending(x => x.CreatedAt);
        }

        return Ok(await ProjectOpportunities(opportunitiesQuery).ToPagedAsync(query));
    }

    [HttpGet("dashboard-summary")]
    [HasPermission("Opportunities.View")]
    public async Task<ActionResult<OpportunityDashboardSummaryDto>> GetDashboardSummary()
    {
        var total = await _dbContext.Opportunities.CountAsync();
        var averageProbability = total == 0
            ? 0m
            : await _dbContext.Opportunities.AverageAsync(x => x.Probability);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        return Ok(new OpportunityDashboardSummaryDto
        {
            TotalOpportunities = total,
            OpenOpportunities = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "OPEN"),
            WonOpportunities = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "WON"),
            LostOpportunities = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "LOST"),
            PipelineValue = await _dbContext.Opportunities
                .Where(x => x.OpportunityStatus.Code == "OPEN" || x.OpportunityStatus.Code == "ON_HOLD")
                .SumAsync(x => x.EstimatedRevenue ?? 0m),
            WeightedPipelineValue = await _dbContext.Opportunities
                .Where(x => x.OpportunityStatus.Code == "OPEN" || x.OpportunityStatus.Code == "ON_HOLD")
                .SumAsync(x => x.WeightedRevenue ?? 0m),
            AverageProbability = Math.Round(averageProbability, 1),
            ClosingThisMonth = await _dbContext.Opportunities.CountAsync(x => x.EstimatedCloseDate >= monthStart && x.EstimatedCloseDate < monthEnd),
            OpportunitiesByStage = await _dbContext.Opportunities
                .GroupBy(x => x.OpportunityStage.Name)
                .Select(x => new LeadDashboardGroupDto { Name = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            OpportunitiesByOwner = await _dbContext.Opportunities
                .GroupBy(x => x.OwnerUser != null ? x.OwnerUser.Email! : x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned")
                .Select(x => new LeadDashboardGroupDto { Name = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            RecentOpportunities = await ProjectOpportunities(_dbContext.Opportunities.OrderByDescending(x => x.CreatedAt).Take(5)).ToListAsync()
        });
    }

    [HttpGet("lookup")]
    [HasPermission("Opportunities.View")]
    public async Task<ActionResult<OpportunityLookupDto>> GetLookup()
    {
        return Ok(new OpportunityLookupDto
        {
            OpportunityStages = await GetLookupOptionsAsync("OPPORTUNITY_STAGE"),
            OpportunityStatuses = await GetLookupOptionsAsync("OPPORTUNITY_STATUS"),
            SalesProcessStages = await GetLookupOptionsAsync("SALES_PROCESS_STAGE"),
            Ratings = await GetLookupOptionsAsync("OPPORTUNITY_RATING"),
            Priorities = await GetLookupOptionsAsync("PRIORITY"),
            Currencies = await GetLookupOptionsAsync("CURRENCY"),
            Sources = await GetLookupOptionsAsync("OPPORTUNITY_SOURCE"),
            WinReasons = await GetLookupOptionsAsync("WIN_REASON"),
            LossReasons = await GetLookupOptionsAsync("LOSS_REASON"),
            ThreatLevels = await GetLookupOptionsAsync("COMPETITOR_THREAT_LEVEL"),
            ActivityTypes = await GetLookupOptionsAsync("ACTIVITY_TYPE"),
            ActivityStatuses = await GetLookupOptionsAsync("ACTIVITY_STATUS")
        });
    }

    [HttpGet("pipeline")]
    [HasPermission("Opportunities.ViewPipeline")]
    public async Task<ActionResult<IReadOnlyCollection<OpportunityPipelineStageDto>>> GetPipeline()
    {
        var stages = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "OPPORTUNITY_STAGE" && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto { Id = x.Id, Name = x.Name, Code = x.Code })
            .ToListAsync();

        var opportunities = await ProjectOpportunities(_dbContext.Opportunities
            .Where(x => x.IsActive && x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED"))
            .ToListAsync();

        var pipeline = stages.Select(stage =>
        {
            var stageOpportunities = opportunities
                .Where(x => x.OpportunityStageId == stage.Id)
                .OrderBy(x => x.EstimatedCloseDate ?? DateTime.MaxValue)
                .ThenByDescending(x => x.WeightedRevenue ?? 0m)
                .ToList();

            return new OpportunityPipelineStageDto
            {
                StageId = stage.Id,
                StageName = stage.Name,
                StageCode = stage.Code,
                Count = stageOpportunities.Count,
                EstimatedRevenue = stageOpportunities.Sum(x => x.EstimatedRevenue ?? 0m),
                WeightedRevenue = stageOpportunities.Sum(x => x.WeightedRevenue ?? 0m),
                Opportunities = stageOpportunities
            };
        }).ToList();

        return Ok(pipeline);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Opportunities.View")]
    public async Task<ActionResult<OpportunityDto>> GetOpportunity(Guid id)
    {
        var opportunity = await GetOpportunityDtoAsync(id);
        return opportunity is null ? NotFound() : Ok(opportunity);
    }

    [HttpGet("{id:guid}/summary")]
    [HasPermission("Opportunities.View")]
    public async Task<ActionResult<OpportunitySummaryDto>> GetSummary(Guid id)
    {
        var opportunity = await GetOpportunityDtoAsync(id);
        if (opportunity is null)
        {
            return NotFound();
        }

        var products = _dbContext.OpportunityProducts.Where(x => x.OpportunityId == id);
        var competitors = _dbContext.OpportunityCompetitors.Where(x => x.OpportunityId == id);
        var activities = _dbContext.OpportunityActivities.Where(x => x.OpportunityId == id);

        return Ok(new OpportunitySummaryDto
        {
            Opportunity = opportunity,
            LatestActivity = await ProjectActivities(activities.OrderByDescending(x => x.ActivityDate).Take(1)).FirstOrDefaultAsync(),
            PrimaryCompetitor = await ProjectCompetitors(competitors.Where(x => x.IsPrimaryCompetitor)).FirstOrDefaultAsync(),
            ProductRevenue = await products.SumAsync(x => x.LineTotal),
            ProductCount = await products.CountAsync(),
            CompetitorCount = await competitors.CountAsync(),
            ActivityCount = await activities.CountAsync()
        });
    }

    [HttpGet("{id:guid}/timeline")]
    [HasPermission("Opportunities.ViewTimeline")]
    public async Task<ActionResult<IReadOnlyCollection<OpportunityTimelineItemDto>>> GetTimeline(Guid id)
    {
        if (!await _dbContext.Opportunities.AnyAsync(x => x.Id == id))
        {
            return NotFound();
        }

        var activities = await _dbContext.OpportunityActivities
            .Where(x => x.OpportunityId == id)
            .Select(x => new OpportunityTimelineItemDto
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
            .Where(x => x.EntityName == nameof(Opportunity) && x.EntityId == id.ToString())
            .Select(x => new OpportunityTimelineItemDto
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

    [HttpPost]
    [HasPermission("Opportunities.Create")]
    public async Task<ActionResult<OpportunityDto>> CreateOpportunity(UpsertOpportunityRequestDto dto)
    {
        var stageId = await ResolveLookupIdAsync(dto.OpportunityStageId, "OPPORTUNITY_STAGE", "QUALIFY");
        var statusId = await ResolveLookupIdAsync(dto.OpportunityStatusId, "OPPORTUNITY_STATUS", "OPEN");
        if (!stageId.HasValue || !statusId.HasValue)
        {
            return BadRequest("Opportunity stage and status lookup values are required.");
        }

        dto.OpportunityStageId = stageId.Value;
        dto.OpportunityStatusId = statusId.Value;

        var validationError = await ValidateReferencesAsync(dto, null);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            OpportunityNumber = await _numberSequenceService.GenerateNextAsync("OPPORTUNITY")
        };

        ApplyOpportunityValues(opportunity, dto);
        var rulesError = await ApplyOutcomeRulesAsync(opportunity);
        if (rulesError is not null)
        {
            return BadRequest(rulesError);
        }

        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        var created = await GetOpportunityDtoAsync(opportunity.Id);
        return created is null ? Problem("Opportunity was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Opportunities.Update")]
    public async Task<IActionResult> UpdateOpportunity(Guid id, UpsertOpportunityRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == id);
        if (opportunity is null)
        {
            return NotFound();
        }

        var stageId = await ResolveLookupIdAsync(dto.OpportunityStageId, "OPPORTUNITY_STAGE", "QUALIFY");
        var statusId = await ResolveLookupIdAsync(dto.OpportunityStatusId, "OPPORTUNITY_STATUS", "OPEN");
        if (!stageId.HasValue || !statusId.HasValue)
        {
            return BadRequest("Opportunity stage and status lookup values are required.");
        }

        dto.OpportunityStageId = stageId.Value;
        dto.OpportunityStatusId = statusId.Value;

        var validationError = await ValidateReferencesAsync(dto, id);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        ApplyOpportunityValues(opportunity, dto);
        if (string.IsNullOrWhiteSpace(opportunity.OpportunityNumber))
        {
            opportunity.OpportunityNumber = await _numberSequenceService.GenerateNextAsync("OPPORTUNITY");
        }

        await RollupProductRevenueIfProductsExistAsync(opportunity);
        var rulesError = await ApplyOutcomeRulesAsync(opportunity);
        if (rulesError is not null)
        {
            return BadRequest(rulesError);
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Opportunities.Delete")]
    public async Task<IActionResult> DeleteOpportunity(Guid id)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == id);
        if (opportunity is null)
        {
            return NotFound();
        }

        opportunity.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/assign-owner")]
    [HasPermission("Opportunities.AssignOwner")]
    public async Task<IActionResult> AssignOwner(Guid id, OpportunityAssignOwnerRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == id);
        if (opportunity is null)
        {
            return NotFound();
        }

        if (dto.OwnerUserId.HasValue == dto.OwnerTeamId.HasValue)
        {
            return BadRequest("Assign the opportunity to either a user or a team.");
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value && !x.IsDeleted))
        {
            return BadRequest("Owner user is invalid.");
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return BadRequest("Owner team is invalid.");
        }

        opportunity.OwnerUserId = dto.OwnerUserId;
        opportunity.OwnerTeamId = dto.OwnerTeamId;
        AddOpportunityActionAudit(id, "AssignOwner", new { dto.OwnerUserId, dto.OwnerTeamId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/change-stage")]
    [HasPermission("Opportunities.ChangeStage")]
    public async Task<IActionResult> ChangeStage(Guid id, OpportunityChangeStageRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities
            .Include(x => x.OpportunityStatus)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (opportunity is null)
        {
            return NotFound();
        }

        if (opportunity.OpportunityStatus.Code is "WON" or "LOST" or "CANCELLED")
        {
            return BadRequest("Closed or cancelled opportunities cannot be moved through the pipeline.");
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.OpportunityStageId && x.LookupCategory.Code == "OPPORTUNITY_STAGE"))
        {
            return BadRequest("Opportunity stage is invalid.");
        }

        opportunity.OpportunityStageId = dto.OpportunityStageId;
        AddOpportunityActionAudit(id, "ChangeStage", new { dto.OpportunityStageId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-won")]
    [HasPermission("Opportunities.MarkWon")]
    public async Task<IActionResult> MarkWon(Guid id, OpportunityMarkWonRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == id);
        if (opportunity is null)
        {
            return NotFound();
        }

        if (!dto.ActualRevenue.HasValue || !dto.ActualCloseDate.HasValue || !dto.WinReasonId.HasValue)
        {
            return BadRequest("Actual revenue, actual close date, and win reason are required.");
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.WinReasonId.Value && x.LookupCategory.Code == "WIN_REASON"))
        {
            return BadRequest("Win reason is invalid.");
        }

        var wonStatusId = await GetLookupValueIdAsync("OPPORTUNITY_STATUS", "WON");
        var closeStageId = await GetLookupValueIdAsync("OPPORTUNITY_STAGE", "CLOSE");
        if (!wonStatusId.HasValue || !closeStageId.HasValue)
        {
            return BadRequest("Won status or close stage is not configured.");
        }

        opportunity.OpportunityStatusId = wonStatusId.Value;
        opportunity.OpportunityStageId = closeStageId.Value;
        opportunity.ActualRevenue = dto.ActualRevenue.Value;
        opportunity.ActualCloseDate = dto.ActualCloseDate.Value;
        opportunity.WinReasonId = dto.WinReasonId.Value;
        opportunity.LossReasonId = null;
        opportunity.LostToCompetitorId = null;
        opportunity.Probability = 100m;
        opportunity.IsActive = false;
        opportunity.Notes = MergeNotes(opportunity.Notes, dto.Notes);
        RecalculateWeightedRevenue(opportunity);
        AddOpportunityActionAudit(id, "MarkWon", new { dto.ActualRevenue, dto.ActualCloseDate, dto.WinReasonId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-lost")]
    [HasPermission("Opportunities.MarkLost")]
    public async Task<IActionResult> MarkLost(Guid id, OpportunityMarkLostRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == id);
        if (opportunity is null)
        {
            return NotFound();
        }

        if (!dto.ActualCloseDate.HasValue || !dto.LossReasonId.HasValue)
        {
            return BadRequest("Actual close date and loss reason are required.");
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.LossReasonId.Value && x.LookupCategory.Code == "LOSS_REASON"))
        {
            return BadRequest("Loss reason is invalid.");
        }

        if (dto.LostToCompetitorId.HasValue && !await _dbContext.OpportunityCompetitors.AnyAsync(x => x.Id == dto.LostToCompetitorId.Value && x.OpportunityId == id))
        {
            return BadRequest("Lost-to competitor must belong to the opportunity.");
        }

        var lostStatusId = await GetLookupValueIdAsync("OPPORTUNITY_STATUS", "LOST");
        var closeStageId = await GetLookupValueIdAsync("OPPORTUNITY_STAGE", "CLOSE");
        if (!lostStatusId.HasValue || !closeStageId.HasValue)
        {
            return BadRequest("Lost status or close stage is not configured.");
        }

        opportunity.OpportunityStatusId = lostStatusId.Value;
        opportunity.OpportunityStageId = closeStageId.Value;
        opportunity.ActualCloseDate = dto.ActualCloseDate.Value;
        opportunity.LossReasonId = dto.LossReasonId.Value;
        opportunity.LostToCompetitorId = dto.LostToCompetitorId;
        opportunity.WinReasonId = null;
        opportunity.Probability = 0m;
        opportunity.IsActive = false;
        opportunity.Notes = MergeNotes(opportunity.Notes, dto.Notes);
        RecalculateWeightedRevenue(opportunity);
        AddOpportunityActionAudit(id, "MarkLost", new { dto.ActualCloseDate, dto.LossReasonId, dto.LostToCompetitorId });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateReferencesAsync(UpsertOpportunityRequestDto dto, Guid? opportunityId)
    {
        if (!await _dbContext.Accounts.AnyAsync(x => x.Id == dto.AccountId))
        {
            return "Account is required.";
        }

        if (dto.ContactId.HasValue && !await _dbContext.Contacts.AnyAsync(x => x.Id == dto.ContactId.Value && x.AccountId == dto.AccountId))
        {
            return "Contact must belong to the selected account.";
        }

        if (dto.LeadId.HasValue && !await _dbContext.Leads.AnyAsync(x => x.Id == dto.LeadId.Value))
        {
            return "Lead is invalid.";
        }

        if (dto.OwnerUserId.HasValue && !await _dbContext.Users.AnyAsync(x => x.Id == dto.OwnerUserId.Value && !x.IsDeleted))
        {
            return "Owner user is invalid.";
        }

        if (dto.OwnerTeamId.HasValue && !await _dbContext.Teams.AnyAsync(x => x.Id == dto.OwnerTeamId.Value))
        {
            return "Owner team is invalid.";
        }

        if (dto.OwnerUserId.HasValue && dto.OwnerTeamId.HasValue)
        {
            return "Choose either an owner user or owner team, not both.";
        }

        if (dto.Probability is < 0m or > 100m)
        {
            return "Probability must be between 0 and 100.";
        }

        var lookupChecks = new (Guid? Id, string Category, string Label)[]
        {
            (dto.OpportunityStageId, "OPPORTUNITY_STAGE", "Opportunity stage"),
            (dto.OpportunityStatusId, "OPPORTUNITY_STATUS", "Opportunity status"),
            (dto.SalesProcessStageId, "SALES_PROCESS_STAGE", "Sales process stage"),
            (dto.RatingId, "OPPORTUNITY_RATING", "Rating"),
            (dto.PriorityId, "PRIORITY", "Priority"),
            (dto.CurrencyId, "CURRENCY", "Currency"),
            (dto.SourceId, "OPPORTUNITY_SOURCE", "Source"),
            (dto.WinReasonId, "WIN_REASON", "Win reason"),
            (dto.LossReasonId, "LOSS_REASON", "Loss reason")
        };

        foreach (var (lookupId, category, label) in lookupChecks)
        {
            if (lookupId.HasValue && lookupId.Value != Guid.Empty &&
                !await _dbContext.LookupValues.AnyAsync(x => x.Id == lookupId.Value && x.LookupCategory.Code == category))
            {
                return $"{label} is invalid.";
            }
        }

        if (dto.LostToCompetitorId.HasValue)
        {
            if (!opportunityId.HasValue)
            {
                return "Lost-to competitor can only be set after the competitor has been created.";
            }

            if (!await _dbContext.OpportunityCompetitors.AnyAsync(x => x.Id == dto.LostToCompetitorId.Value && x.OpportunityId == opportunityId.Value))
            {
                return "Lost-to competitor must belong to the opportunity.";
            }
        }

        return null;
    }

    private async Task<string?> ApplyOutcomeRulesAsync(Opportunity opportunity)
    {
        var statusCode = await _dbContext.LookupValues
            .Where(x => x.Id == opportunity.OpportunityStatusId && x.LookupCategory.Code == "OPPORTUNITY_STATUS")
            .Select(x => x.Code)
            .FirstOrDefaultAsync();

        if (statusCode == "WON")
        {
            if (!opportunity.ActualRevenue.HasValue || !opportunity.ActualCloseDate.HasValue || !opportunity.WinReasonId.HasValue)
            {
                return "Won opportunities require actual revenue, actual close date, and win reason.";
            }

            opportunity.Probability = 100m;
            opportunity.IsActive = false;
            opportunity.LossReasonId = null;
            opportunity.LostToCompetitorId = null;
        }
        else if (statusCode == "LOST")
        {
            if (!opportunity.ActualCloseDate.HasValue || !opportunity.LossReasonId.HasValue)
            {
                return "Lost opportunities require actual close date and loss reason.";
            }

            opportunity.Probability = 0m;
            opportunity.IsActive = false;
            opportunity.WinReasonId = null;
        }

        RecalculateWeightedRevenue(opportunity);
        return null;
    }

    private async Task RollupProductRevenueIfProductsExistAsync(Opportunity opportunity)
    {
        var products = _dbContext.OpportunityProducts.Where(x => x.OpportunityId == opportunity.Id);
        if (!await products.AnyAsync())
        {
            RecalculateWeightedRevenue(opportunity);
            return;
        }

        opportunity.EstimatedRevenue = await products.SumAsync(x => x.LineTotal);
        RecalculateWeightedRevenue(opportunity);
    }

    private async Task<Guid?> ResolveLookupIdAsync(Guid requestedId, string categoryCode, string defaultCode)
    {
        if (requestedId != Guid.Empty)
        {
            return requestedId;
        }

        return await GetLookupValueIdAsync(categoryCode, defaultCode);
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

    private void AddOpportunityActionAudit(Guid opportunityId, string action, object values)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = nameof(Opportunity),
            EntityId = opportunityId.ToString(),
            Action = action,
            NewValues = JsonSerializer.Serialize(values),
            UserId = _currentUserContext.UserId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = _currentUserContext.UserId
        });
    }

    private async Task<OpportunityDto?> GetOpportunityDtoAsync(Guid id)
    {
        return await ProjectOpportunities(_dbContext.Opportunities.Where(x => x.Id == id)).FirstOrDefaultAsync();
    }

    private static void ApplyOpportunityValues(Opportunity opportunity, UpsertOpportunityRequestDto dto)
    {
        opportunity.Topic = dto.Topic.Trim();
        opportunity.AccountId = dto.AccountId;
        opportunity.ContactId = dto.ContactId;
        opportunity.LeadId = dto.LeadId;
        opportunity.OpportunityStageId = dto.OpportunityStageId;
        opportunity.OpportunityStatusId = dto.OpportunityStatusId;
        opportunity.SalesProcessStageId = dto.SalesProcessStageId;
        opportunity.RatingId = dto.RatingId;
        opportunity.PriorityId = dto.PriorityId;
        opportunity.EstimatedRevenue = dto.EstimatedRevenue;
        opportunity.EstimatedCloseDate = dto.EstimatedCloseDate;
        opportunity.Probability = dto.Probability;
        opportunity.ActualRevenue = dto.ActualRevenue;
        opportunity.ActualCloseDate = dto.ActualCloseDate;
        opportunity.CurrencyId = dto.CurrencyId;
        opportunity.SourceId = dto.SourceId;
        opportunity.WinReasonId = dto.WinReasonId;
        opportunity.LossReasonId = dto.LossReasonId;
        opportunity.LostToCompetitorId = dto.LostToCompetitorId;
        opportunity.Description = TrimToNull(dto.Description);
        opportunity.Notes = TrimToNull(dto.Notes);
        opportunity.OwnerUserId = dto.OwnerUserId;
        opportunity.OwnerTeamId = dto.OwnerTeamId;
        opportunity.IsActive = dto.IsActive;
        RecalculateWeightedRevenue(opportunity);
    }

    private static void RecalculateWeightedRevenue(Opportunity opportunity)
    {
        opportunity.WeightedRevenue = opportunity.EstimatedRevenue.HasValue
            ? Math.Round(opportunity.EstimatedRevenue.Value * opportunity.Probability / 100m, 2)
            : null;
    }

    private static string? MergeNotes(string? existing, string? addition)
    {
        var cleaned = TrimToNull(addition);
        if (cleaned is null)
        {
            return existing;
        }

        return string.IsNullOrWhiteSpace(existing) ? cleaned : $"{existing}{Environment.NewLine}{cleaned}";
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IQueryable<OpportunityDto> ProjectOpportunities(IQueryable<Opportunity> query)
    {
        return query.Select(x => new OpportunityDto
        {
            Id = x.Id,
            OpportunityNumber = x.OpportunityNumber,
            Topic = x.Topic,
            AccountId = x.AccountId,
            AccountName = x.Account.Name,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            LeadId = x.LeadId,
            LeadTopic = x.Lead != null ? x.Lead.Topic : null,
            OpportunityStageId = x.OpportunityStageId,
            OpportunityStageName = x.OpportunityStage.Name,
            OpportunityStageCode = x.OpportunityStage.Code,
            OpportunityStatusId = x.OpportunityStatusId,
            OpportunityStatusName = x.OpportunityStatus.Name,
            OpportunityStatusCode = x.OpportunityStatus.Code,
            SalesProcessStageId = x.SalesProcessStageId,
            SalesProcessStageName = x.SalesProcessStage != null ? x.SalesProcessStage.Name : null,
            RatingId = x.RatingId,
            RatingName = x.Rating != null ? x.Rating.Name : null,
            PriorityId = x.PriorityId,
            PriorityName = x.Priority != null ? x.Priority.Name : null,
            EstimatedRevenue = x.EstimatedRevenue,
            EstimatedCloseDate = x.EstimatedCloseDate,
            Probability = x.Probability,
            WeightedRevenue = x.WeightedRevenue,
            ActualRevenue = x.ActualRevenue,
            ActualCloseDate = x.ActualCloseDate,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency != null ? x.Currency.Name : null,
            SourceId = x.SourceId,
            SourceName = x.Source != null ? x.Source.Name : null,
            WinReasonId = x.WinReasonId,
            WinReasonName = x.WinReason != null ? x.WinReason.Name : null,
            LossReasonId = x.LossReasonId,
            LossReasonName = x.LossReason != null ? x.LossReason.Name : null,
            LostToCompetitorId = x.LostToCompetitorId,
            LostToCompetitorName = x.LostToCompetitor != null ? x.LostToCompetitor.CompetitorName : null,
            Description = x.Description,
            Notes = x.Notes,
            IsActive = x.IsActive,
            OwnerUserId = x.OwnerUserId,
            OwnerUserName = x.OwnerUser != null ? x.OwnerUser.Email : null,
            OwnerTeamId = x.OwnerTeamId,
            OwnerTeamName = x.OwnerTeam != null ? x.OwnerTeam.Name : null,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    private static IQueryable<OpportunityProductDto> ProjectProducts(IQueryable<OpportunityProduct> query)
    {
        return query.Select(x => new OpportunityProductDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Description = x.Description,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            DiscountPercent = x.DiscountPercent,
            DiscountAmount = x.DiscountAmount,
            TaxAmount = x.TaxAmount,
            LineTotal = x.LineTotal,
            SortOrder = x.SortOrder
        });
    }

    private static IQueryable<OpportunityCompetitorDto> ProjectCompetitors(IQueryable<OpportunityCompetitor> query)
    {
        return query.Select(x => new OpportunityCompetitorDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            CompetitorName = x.CompetitorName,
            Strengths = x.Strengths,
            Weaknesses = x.Weaknesses,
            ThreatLevelId = x.ThreatLevelId,
            ThreatLevelName = x.ThreatLevel != null ? x.ThreatLevel.Name : null,
            IsPrimaryCompetitor = x.IsPrimaryCompetitor,
            Notes = x.Notes
        });
    }

    private static IQueryable<OpportunityActivityDto> ProjectActivities(IQueryable<OpportunityActivity> query)
    {
        return query.Select(x => new OpportunityActivityDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            ContactId = x.ContactId,
            ContactName = x.Contact != null ? x.Contact.FullName : null,
            ActivityTypeId = x.ActivityTypeId,
            ActivityTypeName = x.ActivityType.Name,
            Subject = x.Subject,
            Description = x.Description,
            ActivityDate = x.ActivityDate,
            DueDate = x.DueDate,
            CompletedDate = x.CompletedDate,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            PriorityId = x.PriorityId,
            PriorityName = x.Priority != null ? x.Priority.Name : null,
            AssignedToUserId = x.AssignedToUserId,
            AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.Email : null
        });
    }
}
