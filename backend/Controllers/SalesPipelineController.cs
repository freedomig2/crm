using System.Globalization;
using System.Text.Json;
using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class SalesPipelineController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public SalesPipelineController(AppDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("api/sales-pipeline/board")]
    [HasPermission("Pipeline.View")]
    public async Task<ActionResult<SalesPipelineBoardDto>> GetBoard([FromQuery] SalesPipelineFilterDto filter)
    {
        var stages = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "OPPORTUNITY_STAGE" && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LookupOptionDto { Id = x.Id, Name = x.Name, Code = x.Code })
            .ToListAsync();

        var opportunities = await ProjectPipelineCards(ApplyPipelineFilters(OpenPipelineQuery(), filter)).ToListAsync();
        var grouped = stages.Select(stage =>
        {
            var stageCards = opportunities
                .Where(x => x.OpportunityStageId == stage.Id)
                .OrderBy(x => x.EstimatedCloseDate ?? DateTime.MaxValue)
                .ThenByDescending(x => x.WeightedRevenue ?? 0m)
                .ToList();

            return new SalesPipelineStageDto
            {
                StageId = stage.Id,
                StageName = stage.Name,
                StageCode = stage.Code,
                Count = stageCards.Count,
                PipelineRevenue = stageCards.Sum(x => x.EstimatedRevenue ?? 0m),
                WeightedRevenue = stageCards.Sum(x => x.WeightedRevenue ?? 0m),
                Opportunities = stageCards
            };
        }).ToList();

        var count = opportunities.Count;
        return Ok(new SalesPipelineBoardDto
        {
            Summary = new SalesPipelineSummaryDto
            {
                TotalOpportunities = count,
                PipelineRevenue = opportunities.Sum(x => x.EstimatedRevenue ?? 0m),
                WeightedPipelineRevenue = opportunities.Sum(x => x.WeightedRevenue ?? 0m),
                AverageProbability = count == 0 ? 0m : Math.Round(opportunities.Average(x => x.Probability), 1),
                AverageDealSize = count == 0 ? 0m : Math.Round(opportunities.Sum(x => x.EstimatedRevenue ?? 0m) / count, 2)
            },
            Stages = grouped
        });
    }

    [HttpPost("api/sales-pipeline/move-stage")]
    [HasPermission("Pipeline.MoveStage")]
    public async Task<IActionResult> MoveStage(PipelineMoveStageRequestDto dto)
    {
        var opportunity = await _dbContext.Opportunities
            .Include(x => x.OpportunityStage)
            .Include(x => x.OpportunityStatus)
            .FirstOrDefaultAsync(x => x.Id == dto.OpportunityId);
        if (opportunity is null)
        {
            return NotFound();
        }

        if (opportunity.OpportunityStatus.Code is "WON" or "LOST" or "CANCELLED")
        {
            return BadRequest("Closed or cancelled opportunities cannot be moved through the pipeline.");
        }

        var newStage = await _dbContext.LookupValues
            .FirstOrDefaultAsync(x => x.Id == dto.NewStageId && x.LookupCategory.Code == "OPPORTUNITY_STAGE");
        if (newStage is null)
        {
            return BadRequest("Opportunity stage is invalid.");
        }

        Guid? salesProcessStageId = dto.SalesProcessStageId;
        if (salesProcessStageId.HasValue)
        {
            var validSalesProcessStage = await _dbContext.LookupValues
                .AnyAsync(x => x.Id == salesProcessStageId.Value && x.LookupCategory.Code == "SALES_PROCESS_STAGE");
            if (!validSalesProcessStage)
            {
                return BadRequest("Sales process stage is invalid.");
            }
        }
        else
        {
            salesProcessStageId = await ResolveSalesProcessStageForOpportunityStageAsync(newStage.Code);
        }

        var previousStageId = opportunity.OpportunityStageId;
        if (previousStageId == dto.NewStageId && opportunity.SalesProcessStageId == salesProcessStageId)
        {
            return NoContent();
        }

        opportunity.OpportunityStageId = dto.NewStageId;
        opportunity.SalesProcessStageId = salesProcessStageId;

        _dbContext.OpportunityStageHistory.Add(new OpportunityStageHistory
        {
            OpportunityId = opportunity.Id,
            PreviousStageId = previousStageId,
            NewStageId = dto.NewStageId,
            ChangedByUserId = _currentUserContext.UserId,
            ChangedAt = DateTime.UtcNow,
            Notes = TrimToNull(dto.Notes)
        });

        await AddStageChangeActivityAsync(opportunity, previousStageId, newStage, dto.Notes);
        AddPipelineAudit(opportunity.Id, "PipelineStageChanged", new
        {
            previousStageId,
            newStageId = dto.NewStageId,
            salesProcessStageId,
            dto.Notes
        });

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/opportunities/{opportunityId:guid}/pipeline-analytics")]
    [HasPermission("Pipeline.View")]
    public async Task<ActionResult<OpportunityPipelineAnalyticsDto>> GetOpportunityPipelineAnalytics(Guid opportunityId)
    {
        var opportunity = await _dbContext.Opportunities
            .Include(x => x.OpportunityStage)
            .FirstOrDefaultAsync(x => x.Id == opportunityId);
        if (opportunity is null)
        {
            return NotFound();
        }

        var history = await ProjectStageHistory(_dbContext.OpportunityStageHistory.Where(x => x.OpportunityId == opportunityId))
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();

        var enteredCurrentStage = history.FirstOrDefault(x => x.NewStageId == opportunity.OpportunityStageId)?.ChangedAt ?? opportunity.CreatedAt;
        var auditTrend = await _dbContext.AuditLogs
            .Where(x => x.EntityName == nameof(Opportunity) && x.EntityId == opportunityId.ToString())
            .OrderBy(x => x.CreatedAt)
            .Select(x => new { x.CreatedAt, x.NewValues })
            .ToListAsync();

        var probabilityTrend = auditTrend
            .Select(x => new TrendPointDto
            {
                Name = x.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Value = ExtractDecimalFromAuditJson(x.NewValues, "Probability") ?? opportunity.Probability,
                Count = 1
            })
            .ToList();

        var revenueTrend = auditTrend
            .Select(x => new TrendPointDto
            {
                Name = x.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Value = ExtractDecimalFromAuditJson(x.NewValues, "EstimatedRevenue") ?? opportunity.EstimatedRevenue ?? 0m,
                Count = 1
            })
            .ToList();

        if (probabilityTrend.Count == 0)
        {
            probabilityTrend.Add(new TrendPointDto { Name = opportunity.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Value = opportunity.Probability, Count = 1 });
        }

        if (revenueTrend.Count == 0)
        {
            revenueTrend.Add(new TrendPointDto { Name = opportunity.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Value = opportunity.EstimatedRevenue ?? 0m, Count = 1 });
        }

        return Ok(new OpportunityPipelineAnalyticsDto
        {
            OpportunityId = opportunity.Id,
            CurrentStageName = opportunity.OpportunityStage.Name,
            DaysInStage = Math.Max(0, (int)(DateTime.UtcNow - enteredCurrentStage).TotalDays),
            StageHistory = history,
            ProbabilityTrend = probabilityTrend,
            RevenueTrend = revenueTrend
        });
    }

    [HttpGet("api/sales/revenue")]
    [HasPermission("SalesPerformance.View")]
    public async Task<ActionResult<RevenueTrackingDto>> GetRevenueTracking()
    {
        var opportunities = await _dbContext.Opportunities
            .Select(x => new
            {
                x.OpportunityStatus.Code,
                x.EstimatedRevenue,
                x.WeightedRevenue,
                x.ActualRevenue,
                x.ActualCloseDate,
                AccountName = x.Account.Name,
                IndustryName = x.Account.IndustryId.HasValue
                    ? _dbContext.LookupValues.Where(v => v.Id == x.Account.IndustryId.Value).Select(v => v.Name).FirstOrDefault()
                    : null,
                OwnerName = x.OwnerUser != null ? x.OwnerUser.Email : x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned"
            })
            .ToListAsync();

        var won = opportunities.Where(x => x.Code == "WON").ToList();
        var open = opportunities.Where(x => x.Code is "OPEN" or "ON_HOLD").ToList();
        var lost = opportunities.Where(x => x.Code == "LOST").ToList();

        return Ok(new RevenueTrackingDto
        {
            WonRevenue = won.Sum(x => x.ActualRevenue ?? 0m),
            LostRevenue = lost.Sum(x => x.EstimatedRevenue ?? 0m),
            PipelineRevenue = open.Sum(x => x.EstimatedRevenue ?? 0m),
            WeightedRevenue = open.Sum(x => x.WeightedRevenue ?? 0m),
            RevenueTrend = BuildMonthlyTrend(won.Where(x => x.ActualCloseDate.HasValue).Select(x => (x.ActualCloseDate!.Value, x.ActualRevenue ?? 0m))),
            RevenueByAccount = won.GroupBy(x => x.AccountName ?? "Not set").Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.ActualRevenue ?? 0m), Count = x.Count() }).OrderByDescending(x => x.Value).Take(10).ToList(),
            RevenueByIndustry = won.GroupBy(x => x.IndustryName ?? "Not set").Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.ActualRevenue ?? 0m), Count = x.Count() }).OrderByDescending(x => x.Value).Take(10).ToList(),
            RevenueBySalesperson = won.GroupBy(x => x.OwnerName ?? "Unassigned").Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.ActualRevenue ?? 0m), Count = x.Count() }).OrderByDescending(x => x.Value).Take(10).ToList()
        });
    }

    [HttpGet("api/sales/performance")]
    [HasPermission("SalesPerformance.View")]
    public async Task<ActionResult<SalesPerformanceDashboardDto>> GetPerformanceDashboard()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var quarterStart = new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var opportunities = await _dbContext.Opportunities
            .Select(x => new
            {
                x.CreatedAt,
                x.EstimatedRevenue,
                x.WeightedRevenue,
                x.ActualRevenue,
                x.ActualCloseDate,
                StatusCode = x.OpportunityStatus.Code,
                StageName = x.OpportunityStage.Name,
                OwnerName = x.OwnerUser != null ? x.OwnerUser.Email : x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned",
                TeamName = x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned",
                IndustryName = x.Account.IndustryId.HasValue
                    ? _dbContext.LookupValues.Where(v => v.Id == x.Account.IndustryId.Value).Select(v => v.Name).FirstOrDefault()
                    : null
            })
            .ToListAsync();

        var won = opportunities.Where(x => x.StatusCode == "WON").ToList();
        var lost = opportunities.Where(x => x.StatusCode == "LOST").ToList();
        var closed = won.Concat(lost).ToList();
        var open = opportunities.Where(x => x.StatusCode is "OPEN" or "ON_HOLD").ToList();
        var closedCount = won.Count + lost.Count;
        var latestForecast = await _dbContext.RevenueForecasts.OrderByDescending(x => x.ForecastDate).FirstOrDefaultAsync();

        var topSalespeople = won
            .GroupBy(x => x.OwnerName ?? "Unassigned")
            .Select(x => new LeaderboardItemDto
            {
                Name = x.Key,
                Revenue = x.Sum(i => i.ActualRevenue ?? 0m),
                WinRate = CalculateWinRate(won.Count(i => i.OwnerName == x.Key), lost.Count(i => i.OwnerName == x.Key)),
                OpportunitiesClosed = x.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        var targets = await _dbContext.SalesTargets.Include(x => x.AssignedTeam).ToListAsync();
        var topTeams = won
            .GroupBy(x => x.TeamName)
            .Select(x => new LeaderboardItemDto
            {
                Name = x.Key,
                Revenue = x.Sum(i => i.ActualRevenue ?? 0m),
                OpportunitiesClosed = x.Count(),
                TargetAchievement = targets.Where(t => t.AssignedTeam != null && t.AssignedTeam.Name == x.Key).DefaultIfEmpty().Average(t => t?.AchievementPercentage ?? 0m)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        return Ok(new SalesPerformanceDashboardDto
        {
            OpenOpportunities = open.Count,
            WonOpportunities = won.Count,
            LostOpportunities = lost.Count,
            WinRate = CalculateWinRate(won.Count, lost.Count),
            AverageDealSize = won.Count == 0 ? 0m : Math.Round(won.Sum(x => x.ActualRevenue ?? 0m) / won.Count, 2),
            AverageSalesCycleDays = closed.Count == 0 ? 0m : Math.Round(closed.Average(x => (decimal)Math.Max(0, ((x.ActualCloseDate ?? now) - x.CreatedAt).TotalDays)), 1),
            RevenueThisMonth = won.Where(x => x.ActualCloseDate >= monthStart).Sum(x => x.ActualRevenue ?? 0m),
            RevenueThisQuarter = won.Where(x => x.ActualCloseDate >= quarterStart).Sum(x => x.ActualRevenue ?? 0m),
            RevenueThisYear = won.Where(x => x.ActualCloseDate >= yearStart).Sum(x => x.ActualRevenue ?? 0m),
            ForecastRevenue = latestForecast?.ForecastRevenue ?? 0m,
            ForecastAccuracy = CalculateForecastAccuracy(latestForecast?.ClosedRevenue ?? 0m, latestForecast?.ForecastRevenue ?? 0m),
            TopSalesperson = topSalespeople.FirstOrDefault()?.Name,
            TopTeam = topTeams.FirstOrDefault()?.Name,
            PipelineByStage = opportunities.GroupBy(x => x.StageName).Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.EstimatedRevenue ?? 0m), Count = x.Count() }).OrderByDescending(x => x.Value).ToList(),
            RevenueTrend = BuildMonthlyTrend(won.Where(x => x.ActualCloseDate.HasValue).Select(x => (x.ActualCloseDate!.Value, x.ActualRevenue ?? 0m))),
            OpportunitiesByOwner = opportunities.GroupBy(x => x.OwnerName ?? "Unassigned").Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.EstimatedRevenue ?? 0m), Count = x.Count() }).OrderByDescending(x => x.Count).Take(10).ToList(),
            OpportunitiesByIndustry = opportunities.GroupBy(x => x.IndustryName ?? "Not set").Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.EstimatedRevenue ?? 0m), Count = x.Count() }).OrderByDescending(x => x.Count).Take(10).ToList(),
            WinRateTrend = await BuildSnapshotTrendAsync("WinRate"),
            ForecastAccuracyTrend = await BuildSnapshotTrendAsync("ForecastAccuracy"),
            TopSalespeople = topSalespeople,
            TopTeams = topTeams
        });
    }

    private IQueryable<Opportunity> OpenPipelineQuery()
    {
        return _dbContext.Opportunities
            .Where(x => x.IsActive && x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED");
    }

    private IQueryable<Opportunity> ApplyPipelineFilters(IQueryable<Opportunity> query, SalesPipelineFilterDto filter)
    {
        if (filter.OwnerUserId.HasValue) query = query.Where(x => x.OwnerUserId == filter.OwnerUserId.Value);
        if (filter.OwnerTeamId.HasValue) query = query.Where(x => x.OwnerTeamId == filter.OwnerTeamId.Value);
        if (filter.StageId.HasValue) query = query.Where(x => x.OpportunityStageId == filter.StageId.Value);
        if (filter.MinRevenue.HasValue) query = query.Where(x => x.EstimatedRevenue >= filter.MinRevenue.Value);
        if (filter.MaxRevenue.HasValue) query = query.Where(x => x.EstimatedRevenue <= filter.MaxRevenue.Value);
        if (filter.MinProbability.HasValue) query = query.Where(x => x.Probability >= filter.MinProbability.Value);
        if (filter.MaxProbability.HasValue) query = query.Where(x => x.Probability <= filter.MaxProbability.Value);
        if (filter.CloseDateFrom.HasValue) query = query.Where(x => x.EstimatedCloseDate >= filter.CloseDateFrom.Value);
        if (filter.CloseDateTo.HasValue) query = query.Where(x => x.EstimatedCloseDate <= filter.CloseDateTo.Value);
        if (filter.AccountId.HasValue) query = query.Where(x => x.AccountId == filter.AccountId.Value);
        if (filter.RatingId.HasValue) query = query.Where(x => x.RatingId == filter.RatingId.Value);
        if (filter.IndustryId.HasValue) query = query.Where(x => x.Account.IndustryId == filter.IndustryId.Value);
        return query;
    }

    private static IQueryable<SalesPipelineCardDto> ProjectPipelineCards(IQueryable<Opportunity> query)
    {
        var now = DateTime.UtcNow;
        return query.Select(x => new SalesPipelineCardDto
        {
            Id = x.Id,
            OpportunityNumber = x.OpportunityNumber,
            Topic = x.Topic,
            AccountName = x.Account.Name,
            EstimatedRevenue = x.EstimatedRevenue,
            Probability = x.Probability,
            WeightedRevenue = x.WeightedRevenue,
            EstimatedCloseDate = x.EstimatedCloseDate,
            OpportunityStageId = x.OpportunityStageId,
            OpportunityStageName = x.OpportunityStage.Name,
            SalesProcessStageId = x.SalesProcessStageId,
            OwnerName = x.OwnerUser != null ? x.OwnerUser.Email : x.OwnerTeam != null ? x.OwnerTeam.Name : null,
            RatingName = x.Rating != null ? x.Rating.Name : null,
            RatingCode = x.Rating != null ? x.Rating.Code : null,
            AgeInDays = (int)(now - x.CreatedAt).TotalDays
        });
    }

    private static IQueryable<OpportunityStageHistoryDto> ProjectStageHistory(IQueryable<OpportunityStageHistory> query)
    {
        return query.Select(x => new OpportunityStageHistoryDto
        {
            Id = x.Id,
            OpportunityId = x.OpportunityId,
            PreviousStageId = x.PreviousStageId,
            PreviousStageName = x.PreviousStage != null ? x.PreviousStage.Name : null,
            NewStageId = x.NewStageId,
            NewStageName = x.NewStage.Name,
            ChangedByUserId = x.ChangedByUserId,
            ChangedByUserName = x.ChangedByUser != null ? x.ChangedByUser.Email : null,
            ChangedAt = x.ChangedAt,
            Notes = x.Notes
        });
    }

    private async Task<Guid?> ResolveSalesProcessStageForOpportunityStageAsync(string opportunityStageCode)
    {
        var code = opportunityStageCode switch
        {
            "QUALIFY" => "IDENTIFY_NEED",
            "DEVELOP" => "BUILD_SOLUTION",
            "PROPOSE" => "PRESENT_PROPOSAL",
            "NEGOTIATE" => "NEGOTIATE_TERMS",
            "CLOSE" => "CLOSED",
            _ => null
        };

        if (code is null)
        {
            return null;
        }

        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "SALES_PROCESS_STAGE" && x.Code == code)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
    }

    private async Task AddStageChangeActivityAsync(Opportunity opportunity, Guid previousStageId, LookupValue newStage, string? notes)
    {
        var activityTypeId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "ACTIVITY_TYPE" && x.Code == "TASK")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
        var statusId = await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "ACTIVITY_STATUS" && x.Code == "COMPLETED")
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();
        var previousStageName = await _dbContext.LookupValues
            .Where(x => x.Id == previousStageId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();

        if (!activityTypeId.HasValue || !statusId.HasValue)
        {
            return;
        }

        _dbContext.OpportunityActivities.Add(new OpportunityActivity
        {
            OpportunityId = opportunity.Id,
            ActivityTypeId = activityTypeId.Value,
            StatusId = statusId.Value,
            Subject = $"Stage changed to {newStage.Name}",
            Description = TrimToNull(notes) ?? $"Moved from {previousStageName ?? "Not set"} to {newStage.Name}.",
            ActivityDate = DateTime.UtcNow,
            CompletedDate = DateTime.UtcNow,
            AssignedToUserId = opportunity.OwnerUserId
        });
    }

    private void AddPipelineAudit(Guid opportunityId, string action, object values)
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

    private async Task<IReadOnlyCollection<TrendPointDto>> BuildSnapshotTrendAsync(string field)
    {
        var snapshots = await _dbContext.SalesPerformanceSnapshots
            .OrderBy(x => x.SnapshotDate)
            .Take(30)
            .ToListAsync();

        return snapshots.Select(x => new TrendPointDto
        {
            Name = x.SnapshotDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Value = field == "ForecastAccuracy" ? x.ForecastAccuracy : x.WinRate,
            Count = 1
        }).ToList();
    }

    private static IReadOnlyCollection<TrendPointDto> BuildMonthlyTrend(IEnumerable<(DateTime Date, decimal Value)> values)
    {
        return values
            .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1))
            .OrderBy(x => x.Key)
            .Select(x => new TrendPointDto
            {
                Name = x.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Value = x.Sum(i => i.Value),
                Count = x.Count()
            })
            .ToList();
    }

    private static decimal CalculateWinRate(int won, int lost)
    {
        var total = won + lost;
        return total == 0 ? 0m : Math.Round((decimal)won / total * 100m, 1);
    }

    private static decimal CalculateForecastAccuracy(decimal closedRevenue, decimal forecastRevenue)
    {
        return forecastRevenue <= 0m ? 0m : Math.Round(closedRevenue / forecastRevenue * 100m, 1);
    }

    private static decimal? ExtractDecimalFromAuditJson(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value))
            {
                return value;
            }

            if (property.ValueKind == JsonValueKind.String && decimal.TryParse(property.GetString(), out value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
