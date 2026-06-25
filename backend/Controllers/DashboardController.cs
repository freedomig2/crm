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
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private static readonly string[] ClosedOpportunityStatusCodes = ["WON", "LOST", "CANCELLED"];
    private static readonly string[] ClosedCaseStatusCodes = ["CLOSED", "RESOLVED"];
    private static readonly string[] CompletedActivityStatusCodes = ["COMPLETED", "DONE"];

    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public DashboardController(AppDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("summary")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var now = DateTime.UtcNow;
        var monthStart = StartOfMonth(now);
        var previousMonthStart = monthStart.AddMonths(-1);
        var quarterStart = StartOfQuarter(now);
        var previousQuarterStart = quarterStart.AddMonths(-3);

        var userContext = await BuildUserContextAsync(now);

        var totalLeads = await _dbContext.Leads.CountAsync();
        var newLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "NEW");
        var qualifiedLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "QUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "QUALIFIED"));
        var convertedLeads = await _dbContext.Leads.CountAsync(x => x.ConvertedAt != null || x.LeadStatus.Code == "CONVERTED");

        var openOpportunities = await _dbContext.Opportunities.CountAsync(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code));
        var pipelineValue = await _dbContext.Opportunities
            .Where(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code))
            .SumAsync(x => x.EstimatedRevenue ?? 0m);
        var weightedPipeline = await _dbContext.Opportunities
            .Where(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code))
            .SumAsync(x => x.WeightedRevenue ?? 0m);

        var wonCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "WON");
        var lostCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "LOST");
        var winRate = CalculateWinRate(wonCount, lostCount);

        var openCases = await _dbContext.ServiceCases.CountAsync(x => x.IsActive && !ClosedCaseStatusCodes.Contains(x.CaseStatus.Code));
        var overdueTasks = await _dbContext.Activities.CountAsync(x => x.IsActive && x.DueDate != null && x.DueDate < now && !CompletedActivityStatusCodes.Contains(x.Status.Code));
        var slaBreaches = await _dbContext.ServiceCases.CountAsync(x => x.IsActive && x.DueAt != null && x.DueAt < now && !ClosedCaseStatusCodes.Contains(x.CaseStatus.Code));

        var revenueThisMonth = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= monthStart)
            .SumAsync(x => x.ActualRevenue ?? 0m);

        var revenueThisQuarter = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= quarterStart)
            .SumAsync(x => x.ActualRevenue ?? 0m);

        var monthLeads = await _dbContext.Leads.CountAsync(x => x.CreatedAt >= monthStart);
        var previousMonthLeads = await _dbContext.Leads.CountAsync(x => x.CreatedAt >= previousMonthStart && x.CreatedAt < monthStart);
        var monthQualifiedLeads = await _dbContext.Leads.CountAsync(x => x.CreatedAt >= monthStart && (x.LeadStatus.Code == "QUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "QUALIFIED")));
        var previousMonthQualifiedLeads = await _dbContext.Leads.CountAsync(x => x.CreatedAt >= previousMonthStart && x.CreatedAt < monthStart && (x.LeadStatus.Code == "QUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "QUALIFIED")));
        var monthConvertedLeads = await _dbContext.Leads.CountAsync(x => x.ConvertedAt != null && x.ConvertedAt >= monthStart);
        var previousMonthConvertedLeads = await _dbContext.Leads.CountAsync(x => x.ConvertedAt != null && x.ConvertedAt >= previousMonthStart && x.ConvertedAt < monthStart);

        var previousMonthRevenue = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= previousMonthStart && x.ActualCloseDate < monthStart)
            .SumAsync(x => x.ActualRevenue ?? 0m);

        var previousQuarterRevenue = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= previousQuarterStart && x.ActualCloseDate < quarterStart)
            .SumAsync(x => x.ActualRevenue ?? 0m);

        var opportunitiesClosingSoon = await _dbContext.Opportunities
            .Where(x => x.IsActive && x.EstimatedCloseDate != null && x.EstimatedCloseDate >= now && x.EstimatedCloseDate <= now.AddDays(14))
            .OrderBy(x => x.EstimatedCloseDate)
            .Take(8)
            .Select(x => new DashboardOpportunityListItemDto
            {
                Id = x.Id,
                OpportunityNumber = x.OpportunityNumber,
                Topic = x.Topic,
                AccountName = x.Account.Name,
                EstimatedRevenue = x.EstimatedRevenue,
                Probability = x.Probability,
                EstimatedCloseDate = x.EstimatedCloseDate,
                StageName = x.OpportunityStage.Name,
            })
            .ToListAsync();

        var upcomingFollowUps = await _dbContext.Activities
            .Where(x => x.IsActive && x.DueDate != null && x.DueDate >= now && x.DueDate <= now.AddDays(7) && !CompletedActivityStatusCodes.Contains(x.Status.Code))
            .OrderBy(x => x.DueDate)
            .Take(8)
            .Select(x => new DashboardTaskItemDto
            {
                Id = x.Id,
                Subject = x.Subject,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                DueDate = x.DueDate,
                StatusName = x.Status.Name,
                RelatedRecord = BuildRelatedRecord(
                    x.Account != null ? x.Account.Name : null,
                    x.Contact != null ? x.Contact.FullName : null,
                    x.Lead != null ? x.Lead.Topic : null,
                    x.Opportunity != null ? x.Opportunity.Topic : null,
                    x.Case != null ? x.Case.CaseNumber : null),
                IsOverdue = false,
            })
            .ToListAsync();

        var slaAlerts = await _dbContext.ServiceCases
            .Where(x => x.IsActive && x.DueAt != null && x.DueAt < now && !ClosedCaseStatusCodes.Contains(x.CaseStatus.Code))
            .OrderBy(x => x.DueAt)
            .Take(8)
            .Select(x => new DashboardSlaAlertItemDto
            {
                Id = x.Id,
                CaseNumber = x.CaseNumber,
                Title = x.Subject,
                PriorityName = x.Priority.Name,
                DueAt = x.DueAt,
                AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.Email : null,
            })
            .ToListAsync();

        var recentLeads = await _dbContext.Leads
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .Select(x => new DashboardLeadListItemDto
            {
                Id = x.Id,
                LeadNumber = x.LeadNumber,
                Topic = x.Topic,
                CompanyName = x.CompanyName,
                StatusName = x.LeadStatus.Name,
                Score = x.Score,
                OwnerName = x.OwnerUser != null ? x.OwnerUser.Email : x.OwnerTeam != null ? x.OwnerTeam.Name : null,
            })
            .ToListAsync();

        var kpis = new List<DashboardKpiDto>
        {
            BuildKpi("total-leads", "PeopleAddRegular", "Total Leads", totalLeads, totalLeads - monthLeads, "/leads"),
            BuildKpi("qualified-leads", "PeopleCommunityRegular", "Qualified Leads", qualifiedLeads, qualifiedLeads - monthQualifiedLeads, "/leads"),
            BuildKpi("converted-leads", "ArrowSwapRegular", "Converted Leads", convertedLeads, convertedLeads - monthConvertedLeads, "/leads"),
            BuildKpi("open-opportunities", "DataPieRegular", "Open Opportunities", openOpportunities, Math.Max(0, openOpportunities - (monthLeads - monthConvertedLeads)), "/opportunities"),
            BuildKpi("pipeline-value", "ArrowTrendingRegular", "Pipeline Value", pipelineValue, Math.Max(0m, pipelineValue - weightedPipeline), "/sales/pipeline"),
            BuildKpi("weighted-pipeline", "BranchForkRegular", "Weighted Pipeline", weightedPipeline, Math.Max(0m, weightedPipeline - (weightedPipeline * 0.08m)), "/sales/pipeline"),
            BuildKpi("revenue-month", "MoneyRegular", "Revenue This Month", revenueThisMonth, previousMonthRevenue, "/sales/revenue"),
            BuildKpi("revenue-quarter", "BuildingRetailMoneyRegular", "Revenue This Quarter", revenueThisQuarter, previousQuarterRevenue, "/sales/revenue"),
            BuildKpi("open-cases", "HeadsetRegular", "Open Cases", openCases, Math.Max(0, openCases - 2), "/service/cases", false),
            BuildKpi("sla-breaches", "WarningRegular", "SLA Breaches", slaBreaches, Math.Max(0, slaBreaches - 1), "/service/cases", false),
            BuildKpi("open-tasks", "ClipboardTaskRegular", "Open Tasks", userContext.OpenTasks, Math.Max(0, userContext.OpenTasks - userContext.OverdueActivities), "/dashboard/my-open-tasks", false),
            BuildKpi("win-rate", "TrophyRegular", "Win Rate", winRate, Math.Max(0m, winRate - 3m), "/sales/performance")
        };

        return Ok(new DashboardSummaryDto
        {
            Welcome = new DashboardWelcomeDto
            {
                UserName = userContext.UserName,
                DateLabel = now.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture),
                CurrentRole = userContext.Role,
                BusinessUnit = userContext.BusinessUnit,
                Team = userContext.Team,
                OpenTasks = userContext.OpenTasks,
                OverdueActivities = userContext.OverdueActivities,
                OpportunitiesClosingThisWeek = userContext.OpportunitiesClosingThisWeek,
                SlaBreaches = userContext.SlaBreaches,
                HasManagementAccess = userContext.HasManagementAccess,
            },
            Kpis = kpis,
            TotalLeads = totalLeads,
            NewLeads = newLeads,
            QualifiedLeads = qualifiedLeads,
            ConvertedLeads = convertedLeads,
            OpenOpportunities = openOpportunities,
            PipelineValue = pipelineValue,
            WeightedPipeline = weightedPipeline,
            WinRate = winRate,
            OpenCases = openCases,
            OverdueTasks = overdueTasks,
            RevenueThisMonth = revenueThisMonth,
            SlaBreaches = slaBreaches,
            RecentLeads = recentLeads,
            OpportunitiesClosingSoon = opportunitiesClosingSoon,
            UpcomingFollowUps = upcomingFollowUps,
            SlaAlerts = slaAlerts,
        });
    }

    [HttpGet("my-work")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardMyWorkDto>> GetMyWork()
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var userId = _currentUserContext.UserId.Value;
        var now = DateTime.UtcNow;

        var assignedLeads = await _dbContext.Leads
            .Where(x => x.OwnerUserId == userId || x.AssignedToUserId == userId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(10)
            .Select(x => new DashboardLeadListItemDto
            {
                Id = x.Id,
                LeadNumber = x.LeadNumber,
                Topic = x.Topic,
                CompanyName = x.CompanyName,
                StatusName = x.LeadStatus.Name,
                Score = x.Score,
                OwnerName = x.OwnerUser != null ? x.OwnerUser.Email : null,
            })
            .ToListAsync();

        var assignedOpportunities = await _dbContext.Opportunities
            .Where(x => x.OwnerUserId == userId)
            .OrderByDescending(x => x.EstimatedCloseDate ?? DateTime.MaxValue)
            .Take(10)
            .Select(x => new DashboardOpportunityListItemDto
            {
                Id = x.Id,
                OpportunityNumber = x.OpportunityNumber,
                Topic = x.Topic,
                AccountName = x.Account.Name,
                EstimatedRevenue = x.EstimatedRevenue,
                Probability = x.Probability,
                EstimatedCloseDate = x.EstimatedCloseDate,
                StageName = x.OpportunityStage.Name,
            })
            .ToListAsync();

        var assignedCases = await _dbContext.ServiceCases
            .Where(x => x.AssignedToUserId == userId || x.OwnerUserId == userId)
            .OrderBy(x => x.DueAt ?? DateTime.MaxValue)
            .Take(10)
            .Select(x => new DashboardCaseListItemDto
            {
                Id = x.Id,
                CaseNumber = x.CaseNumber,
                Subject = x.Subject,
                PriorityName = x.Priority.Name,
                StatusName = x.CaseStatus.Name,
                DueAt = x.DueAt,
                AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.Email : null,
            })
            .ToListAsync();

        var assignedActivities = await _dbContext.Activities
            .Where(x => x.AssignedToUserId == userId || x.OwnerUserId == userId)
            .OrderByDescending(x => x.ActivityDate)
            .Take(20)
            .Select(x => new DashboardActivityItemDto
            {
                Id = x.Id,
                ActivityNumber = x.ActivityNumber,
                Subject = x.Subject,
                ActivityTypeName = x.ActivityType.Name,
                StatusName = x.Status.Name,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                DueDate = x.DueDate,
                ActivityDate = x.ActivityDate,
                RelatedRecord = BuildRelatedRecord(
                    x.Account != null ? x.Account.Name : null,
                    x.Contact != null ? x.Contact.FullName : null,
                    x.Lead != null ? x.Lead.Topic : null,
                    x.Opportunity != null ? x.Opportunity.Topic : null,
                    x.Case != null ? x.Case.CaseNumber : null),
            })
            .ToListAsync();

        var pendingApprovals = await _dbContext.Quotes
            .Where(x => x.ApprovedById == userId || (x.ApprovalStatus.Code != "APPROVED" && x.OwnerUserId == userId))
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(10)
            .Select(x => new DashboardApprovalItemDto
            {
                Id = x.Id,
                ReferenceNumber = x.QuoteNumber,
                Type = "Quote",
                AccountName = x.Account.Name,
                TotalAmount = x.TotalAmount,
                ApprovalStatusName = x.ApprovalStatus.Name,
            })
            .ToListAsync();

        var overdueTasks = await _dbContext.Activities
            .Where(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && x.DueDate != null && x.DueDate < now && !CompletedActivityStatusCodes.Contains(x.Status.Code))
            .OrderBy(x => x.DueDate)
            .Take(10)
            .Select(x => new DashboardTaskItemDto
            {
                Id = x.Id,
                Subject = x.Subject,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                DueDate = x.DueDate,
                StatusName = x.Status.Name,
                RelatedRecord = BuildRelatedRecord(
                    x.Account != null ? x.Account.Name : null,
                    x.Contact != null ? x.Contact.FullName : null,
                    x.Lead != null ? x.Lead.Topic : null,
                    x.Opportunity != null ? x.Opportunity.Topic : null,
                    x.Case != null ? x.Case.CaseNumber : null),
                IsOverdue = true,
            })
            .ToListAsync();

        return Ok(new DashboardMyWorkDto
        {
            AssignedLeads = assignedLeads,
            AssignedOpportunities = assignedOpportunities,
            AssignedCases = assignedCases,
            AssignedActivities = assignedActivities,
            PendingApprovals = pendingApprovals,
            OverdueTasks = overdueTasks,
        });
    }

    [HttpGet("my-activities")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardMyActivitiesDto>> GetMyActivities([FromQuery] DashboardMyActivitiesFilterDto query)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var userId = _currentUserContext.UserId.Value;
        var activities = _dbContext.Activities.Where(x => x.AssignedToUserId == userId || x.OwnerUserId == userId);

        if (query.ActivityTypeId.HasValue)
        {
            activities = activities.Where(x => x.ActivityTypeId == query.ActivityTypeId.Value);
        }

        if (query.StatusId.HasValue)
        {
            activities = activities.Where(x => x.StatusId == query.StatusId.Value);
        }

        if (query.PriorityId.HasValue)
        {
            activities = activities.Where(x => x.PriorityId == query.PriorityId.Value);
        }

        if (query.DueDateFrom.HasValue)
        {
            activities = activities.Where(x => x.DueDate != null && x.DueDate >= query.DueDateFrom.Value);
        }

        if (query.DueDateTo.HasValue)
        {
            activities = activities.Where(x => x.DueDate != null && x.DueDate <= query.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            activities = activities.Where(x =>
                x.ActivityNumber.ToLower().Contains(search) ||
                x.Subject.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.RelatedRecord))
        {
            var relatedSearch = query.RelatedRecord.Trim().ToLower();
            activities = activities.Where(x =>
                (x.Account != null && x.Account.Name.ToLower().Contains(relatedSearch)) ||
                (x.Contact != null && x.Contact.FullName != null && x.Contact.FullName.ToLower().Contains(relatedSearch)) ||
                (x.Lead != null && x.Lead.Topic.ToLower().Contains(relatedSearch)) ||
                (x.Opportunity != null && x.Opportunity.Topic.ToLower().Contains(relatedSearch)) ||
                (x.Case != null && x.Case.CaseNumber.ToLower().Contains(relatedSearch)));
        }

        var projected = activities.Select(x => new DashboardActivityItemDto
        {
            Id = x.Id,
            ActivityNumber = x.ActivityNumber,
            Subject = x.Subject,
            ActivityTypeName = x.ActivityType.Name,
            StatusName = x.Status.Name,
            PriorityName = x.Priority != null ? x.Priority.Name : null,
            DueDate = x.DueDate,
            ActivityDate = x.ActivityDate,
            RelatedRecord = BuildRelatedRecord(
                x.Account != null ? x.Account.Name : null,
                x.Contact != null ? x.Contact.FullName : null,
                x.Lead != null ? x.Lead.Topic : null,
                x.Opportunity != null ? x.Opportunity.Topic : null,
                x.Case != null ? x.Case.CaseNumber : null),
        });

        var totalCount = await projected.CountAsync();
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var items = await projected
            .OrderByDescending(x => x.ActivityDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var byStatus = await activities
            .GroupBy(x => x.Status.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return Ok(new DashboardMyActivitiesDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            ActivitiesByStatus = byStatus,
        });
    }

    [HttpGet("my-tasks")]
    [HasPermission("Dashboard.View")]
    public Task<ActionResult<DashboardMyOpenTasksDto>> GetMyTasks([FromQuery] DashboardMyOpenTasksFilterDto query)
    {
        return GetMyOpenTasks(query);
    }

    [HttpGet("my-open-tasks")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardMyOpenTasksDto>> GetMyOpenTasks([FromQuery] DashboardMyOpenTasksFilterDto query)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var userId = _currentUserContext.UserId.Value;
        var now = DateTime.UtcNow;

        var tasks = _dbContext.Activities
            .Where(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && !CompletedActivityStatusCodes.Contains(x.Status.Code));

        if (query.PriorityId.HasValue)
        {
            tasks = tasks.Where(x => x.PriorityId == query.PriorityId.Value);
        }

        if (query.DueDateFrom.HasValue)
        {
            tasks = tasks.Where(x => x.DueDate != null && x.DueDate >= query.DueDateFrom.Value);
        }

        if (query.DueDateTo.HasValue)
        {
            tasks = tasks.Where(x => x.DueDate != null && x.DueDate <= query.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            tasks = tasks.Where(x =>
                x.ActivityNumber.ToLower().Contains(search) ||
                x.Subject.ToLower().Contains(search) ||
                (x.Description ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await tasks.CountAsync();
        var overdueCount = await tasks.CountAsync(x => x.DueDate != null && x.DueDate < now);

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var items = await tasks
            .OrderBy(x => x.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(x => x.ActivityDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DashboardTaskItemDto
            {
                Id = x.Id,
                Subject = x.Subject,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                DueDate = x.DueDate,
                StatusName = x.Status.Name,
                RelatedRecord = BuildRelatedRecord(
                    x.Account != null ? x.Account.Name : null,
                    x.Contact != null ? x.Contact.FullName : null,
                    x.Lead != null ? x.Lead.Topic : null,
                    x.Opportunity != null ? x.Opportunity.Topic : null,
                    x.Case != null ? x.Case.CaseNumber : null),
                IsOverdue = x.DueDate != null && x.DueDate < now,
            })
            .ToListAsync();

        return Ok(new DashboardMyOpenTasksDto
        {
            Items = items,
            TotalCount = totalCount,
            OverdueCount = overdueCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    [HttpGet("pipeline")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardPipelineDto>> GetPipeline()
    {
        var leadCount = await _dbContext.Leads.CountAsync();
        var qualifiedLeadCount = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "QUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "QUALIFIED"));
        var opportunityCount = await _dbContext.Opportunities.CountAsync(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code));
        var quoteCount = await _dbContext.Quotes.CountAsync(x => x.IsActive && x.QuoteStatus.Code != "CANCELLED");
        var orderCount = await _dbContext.Orders.CountAsync(x => x.IsActive && x.OrderStatus.Code != "CANCELLED");
        var invoiceCount = await _dbContext.Invoices.CountAsync(x => x.IsActive);

        var stageDistribution = await _dbContext.Opportunities
            .Where(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code))
            .GroupBy(x => x.OpportunityStage.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Sum(i => i.EstimatedRevenue ?? 0m),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var latestSnapshot = await _dbContext.SalesPerformanceSnapshots
            .OrderByDescending(x => x.SnapshotDate)
            .Select(x => x.ForecastAccuracy)
            .FirstOrDefaultAsync();

        var forecastAccuracy = latestSnapshot;
        if (forecastAccuracy <= 0m)
        {
            var now = DateTime.UtcNow;
            var monthStart = StartOfMonth(now);
            var previousMonthStart = monthStart.AddMonths(-1);

            var actual = await _dbContext.Opportunities
                .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= previousMonthStart && x.ActualCloseDate < monthStart)
                .SumAsync(x => x.ActualRevenue ?? 0m);

            var forecast = await _dbContext.RevenueForecasts
                .Where(x => x.ForecastDate >= previousMonthStart && x.ForecastDate < monthStart)
                .SumAsync(x => x.ForecastRevenue);

            forecastAccuracy = CalculateForecastAccuracy(actual, forecast);
        }

        return Ok(new DashboardPipelineDto
        {
            FunnelStages =
            [
                new DashboardChartPointDto { Name = "Lead", Count = leadCount, Value = leadCount },
                new DashboardChartPointDto { Name = "Qualified", Count = qualifiedLeadCount, Value = qualifiedLeadCount },
                new DashboardChartPointDto { Name = "Opportunity", Count = opportunityCount, Value = opportunityCount },
                new DashboardChartPointDto { Name = "Quote", Count = quoteCount, Value = quoteCount },
                new DashboardChartPointDto { Name = "Order", Count = orderCount, Value = orderCount },
                new DashboardChartPointDto { Name = "Invoice", Count = invoiceCount, Value = invoiceCount },
            ],
            OpportunityStageDistribution = stageDistribution,
            ForecastAccuracyPercent = Math.Round(forecastAccuracy, 1),
        });
    }

    [HttpGet("revenue")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardRevenueDto>> GetRevenue()
    {
        var now = DateTime.UtcNow;
        var monthStart = StartOfMonth(now);
        var quarterStart = StartOfQuarter(now);

        var months = Enumerable.Range(0, 12)
            .Select(offset => StartOfMonth(now).AddMonths(-(11 - offset)))
            .ToList();

        var minMonth = months.First();

        var actualRows = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= minMonth)
            .Select(x => new
            {
                Year = x.ActualCloseDate!.Value.Year,
                Month = x.ActualCloseDate.Value.Month,
                Revenue = x.ActualRevenue ?? 0m,
            })
            .ToListAsync();

        var forecastRows = await _dbContext.RevenueForecasts
            .Where(x => x.ForecastDate >= minMonth)
            .Select(x => new
            {
                Year = x.ForecastDate.Year,
                Month = x.ForecastDate.Month,
                Revenue = x.ForecastRevenue,
            })
            .ToListAsync();

        var actualByMonth = actualRows
            .GroupBy(x => new { x.Year, x.Month })
            .ToDictionary(x => (x.Key.Year, x.Key.Month), x => x.Sum(i => i.Revenue));

        var forecastByMonth = forecastRows
            .GroupBy(x => new { x.Year, x.Month })
            .ToDictionary(x => (x.Key.Year, x.Key.Month), x => x.Sum(i => i.Revenue));

        var trend = months.Select(month => new DashboardRevenueTrendPointDto
        {
            Month = month.ToString("yyyy-MM"),
            ActualRevenue = actualByMonth.TryGetValue((month.Year, month.Month), out var actual) ? actual : 0m,
            ForecastRevenue = forecastByMonth.TryGetValue((month.Year, month.Month), out var forecast) ? forecast : 0m,
        }).ToList();

        var revenueThisMonth = trend.Last().ActualRevenue;
        var revenueThisQuarter = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= quarterStart)
            .SumAsync(x => x.ActualRevenue ?? 0m);

        return Ok(new DashboardRevenueDto
        {
            RevenueThisMonth = revenueThisMonth,
            RevenueThisQuarter = revenueThisQuarter,
            MonthlyTrend = trend,
        });
    }

    [HttpGet("cases")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardServiceDto>> GetCases()
    {
        var now = DateTime.UtcNow;

        var byPriority = await _dbContext.ServiceCases
            .Where(x => x.IsActive)
            .GroupBy(x => x.Priority.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var byStatus = await _dbContext.ServiceCases
            .Where(x => x.IsActive)
            .GroupBy(x => x.CaseStatus.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var closedCases = await _dbContext.ServiceCases
            .Where(x => x.IsActive && x.DueAt != null && (x.ResolvedAt != null || x.ClosedAt != null))
            .Select(x => new
            {
                x.DueAt,
                CompletedAt = x.ResolvedAt ?? x.ClosedAt,
            })
            .ToListAsync();

        var compliantCount = closedCases.Count(x => x.CompletedAt != null && x.DueAt != null && x.CompletedAt <= x.DueAt);
        var slaCompliance = closedCases.Count == 0 ? 100m : Math.Round((decimal)compliantCount * 100m / closedCases.Count, 1);

        var casesRequiringAttention = await _dbContext.ServiceCases
            .Where(x => x.IsActive
                && !ClosedCaseStatusCodes.Contains(x.CaseStatus.Code)
                && (
                    x.DueAt == null
                    || x.DueAt < now
                    || x.DueAt <= now.AddDays(2)
                    || x.Priority.Code == "HIGH"
                    || x.Priority.Code == "URGENT"
                    || x.Priority.Code == "CRITICAL"))
            .OrderBy(x => x.DueAt ?? DateTime.MaxValue)
            .ThenByDescending(x => x.Priority.Name)
            .Take(12)
            .Select(x => new DashboardCaseListItemDto
            {
                Id = x.Id,
                CaseNumber = x.CaseNumber,
                Subject = x.Subject,
                PriorityName = x.Priority.Name,
                StatusName = x.CaseStatus.Name,
                DueAt = x.DueAt,
                AssignedToName = x.AssignedToUser != null ? x.AssignedToUser.Email : null,
            })
            .ToListAsync();

        return Ok(new DashboardServiceDto
        {
            CasesByPriority = byPriority,
            CasesByStatus = byStatus,
            SlaCompliancePercent = slaCompliance,
            CasesRequiringAttention = casesRequiringAttention,
        });
    }

    [HttpGet("customers")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardCustomersDto>> GetCustomers()
    {
        var now = DateTime.UtcNow;
        var last90Days = now.AddDays(-90);
        var previous90Days = now.AddDays(-180);

        var topCustomerRows = await _dbContext.Invoices
            .Where(x => x.InvoiceDate != null)
            .GroupBy(x => new { x.AccountId, x.Account.Name })
            .Select(x => new
            {
                x.Key.AccountId,
                AccountName = x.Key.Name,
                Revenue = x.Sum(i => i.TotalAmount),
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();

        var openOpportunitiesByAccount = await _dbContext.Opportunities
            .Where(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code))
            .GroupBy(x => x.AccountId)
            .Select(x => new { AccountId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.AccountId, x => x.Count);

        var topCustomers = topCustomerRows
            .Select(x => new DashboardCustomerInsightItemDto
            {
                AccountId = x.AccountId,
                AccountName = x.AccountName,
                Revenue = x.Revenue,
                OpenOpportunities = openOpportunitiesByAccount.TryGetValue(x.AccountId, out var count) ? count : 0,
            })
            .ToList();

        var accounts = await _dbContext.Accounts
            .Select(x => new { x.Id, x.Name, x.CreatedAt })
            .ToListAsync();

        var latestAccountActivity = await _dbContext.Activities
            .Where(x => x.AccountId != null)
            .GroupBy(x => x.AccountId!.Value)
            .Select(x => new { AccountId = x.Key, LastActivityAt = x.Max(i => i.ActivityDate) })
            .ToDictionaryAsync(x => x.AccountId, x => x.LastActivityAt);

        var escalatedCasesByAccount = await _dbContext.ServiceCases
            .Where(x => x.EscalatedToUserId != null && !ClosedCaseStatusCodes.Contains(x.CaseStatus.Code))
            .GroupBy(x => x.AccountId)
            .Select(x => new { AccountId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.AccountId, x => x.Count);

        var revenueByAccountPeriod = await _dbContext.Invoices
            .Where(x => x.InvoiceDate != null && x.InvoiceDate >= previous90Days)
            .GroupBy(x => new
            {
                x.AccountId,
                IsRecent = x.InvoiceDate >= last90Days,
            })
            .Select(x => new
            {
                x.Key.AccountId,
                x.Key.IsRecent,
                Revenue = x.Sum(i => i.TotalAmount),
            })
            .ToListAsync();

        var revenueRecent = revenueByAccountPeriod
            .Where(x => x.IsRecent)
            .ToDictionary(x => x.AccountId, x => x.Revenue);
        var revenuePrevious = revenueByAccountPeriod
            .Where(x => !x.IsRecent)
            .ToDictionary(x => x.AccountId, x => x.Revenue);

        var atRiskCustomers = new List<DashboardCustomerInsightItemDto>();
        foreach (var account in accounts)
        {
            var reasons = new List<string>();

            if (!latestAccountActivity.TryGetValue(account.Id, out var lastActivityAt) || lastActivityAt < last90Days)
            {
                reasons.Add("No activity in 90 days");
            }

            if (escalatedCasesByAccount.TryGetValue(account.Id, out var escalatedCount) && escalatedCount > 0)
            {
                reasons.Add("Open escalations");
            }

            var recentRevenue = revenueRecent.TryGetValue(account.Id, out var recent) ? recent : 0m;
            var previousRevenue = revenuePrevious.TryGetValue(account.Id, out var previous) ? previous : 0m;
            if (previousRevenue > 0m && recentRevenue < previousRevenue)
            {
                reasons.Add("Revenue decline");
            }

            if (reasons.Count == 0)
            {
                continue;
            }

            atRiskCustomers.Add(new DashboardCustomerInsightItemDto
            {
                AccountId = account.Id,
                AccountName = account.Name,
                Revenue = recentRevenue,
                OpenOpportunities = openOpportunitiesByAccount.TryGetValue(account.Id, out var count) ? count : 0,
                Reason = string.Join("; ", reasons),
            });
        }

        var newCustomers = await _dbContext.Accounts
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new DashboardCustomerInsightItemDto
            {
                AccountId = x.Id,
                AccountName = x.Name,
                Revenue = 0m,
                OpenOpportunities = 0,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();

        foreach (var customer in newCustomers)
        {
            customer.Revenue = revenueRecent.TryGetValue(customer.AccountId, out var recent) ? recent : 0m;
            customer.OpenOpportunities = openOpportunitiesByAccount.TryGetValue(customer.AccountId, out var count) ? count : 0;
        }

        return Ok(new DashboardCustomersDto
        {
            TopCustomers = topCustomers,
            AtRiskCustomers = atRiskCustomers.OrderByDescending(x => x.Revenue).Take(10).ToList(),
            NewCustomers = newCustomers,
        });
    }

    [HttpGet("management")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardManagementDto>> GetManagement()
    {
        var userContext = await BuildUserContextAsync(DateTime.UtcNow);
        if (!userContext.HasManagementAccess)
        {
            return Ok(new DashboardManagementDto { IsVisible = false });
        }

        var closedByOwner = await _dbContext.Opportunities
            .Where(x => x.OwnerUserId != null && (x.OpportunityStatus.Code == "WON" || x.OpportunityStatus.Code == "LOST"))
            .GroupBy(x => new { x.OwnerUserId, OwnerName = x.OwnerUser != null ? x.OwnerUser.Email : "Unknown" })
            .Select(x => new
            {
                UserId = x.Key.OwnerUserId!.Value,
                x.Key.OwnerName,
                Won = x.Count(i => i.OpportunityStatus.Code == "WON"),
                Lost = x.Count(i => i.OpportunityStatus.Code == "LOST"),
                Revenue = x.Where(i => i.OpportunityStatus.Code == "WON").Sum(i => i.ActualRevenue ?? 0m),
            })
            .ToListAsync();

        var topSalespeople = closedByOwner
            .Select(x => new DashboardSalespersonPerformanceDto
            {
                UserId = x.UserId,
                UserName = x.OwnerName ?? "Unknown",
                Revenue = x.Revenue,
                OpportunitiesWon = x.Won,
                WinRate = CalculateWinRate(x.Won, x.Lost),
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        var teamPerformanceRows = await _dbContext.SalesTargets
            .Where(x => x.AssignedTeamId != null)
            .GroupBy(x => new { x.AssignedTeamId, TeamName = x.AssignedTeam != null ? x.AssignedTeam.Name : "Unassigned" })
            .Select(x => new DashboardTeamPerformanceDto
            {
                TeamId = x.Key.AssignedTeamId!.Value,
                TeamName = x.Key.TeamName,
                Target = x.Sum(i => i.TargetAmount),
                Actual = x.Sum(i => i.ActualAmount),
                AchievementPercent = x.Sum(i => i.TargetAmount) <= 0m ? 0m : Math.Round((x.Sum(i => i.ActualAmount) / x.Sum(i => i.TargetAmount)) * 100m, 1),
            })
            .OrderByDescending(x => x.AchievementPercent)
            .Take(10)
            .ToListAsync();

        var conversionMonths = Enumerable.Range(0, 12)
            .Select(offset => StartOfMonth(DateTime.UtcNow).AddMonths(-(11 - offset)))
            .ToList();
        var conversionMinMonth = conversionMonths.First();

        var convertedRows = await _dbContext.Leads
            .Where(x => x.ConvertedAt != null && x.ConvertedAt >= conversionMinMonth)
            .Select(x => new { Year = x.ConvertedAt!.Value.Year, Month = x.ConvertedAt.Value.Month })
            .ToListAsync();

        var conversionMap = convertedRows
            .GroupBy(x => (x.Year, x.Month))
            .ToDictionary(x => x.Key, x => x.Count());

        var leadConversionTrend = conversionMonths
            .Select(month => new DashboardChartPointDto
            {
                Name = month.ToString("yyyy-MM"),
                Count = conversionMap.TryGetValue((month.Year, month.Month), out var count) ? count : 0,
                Value = conversionMap.TryGetValue((month.Year, month.Month), out var value) ? value : 0,
            })
            .ToList();

        var revenueByTeam = await _dbContext.Opportunities
            .Where(x => x.OwnerTeamId != null && x.OpportunityStatus.Code == "WON")
            .GroupBy(x => x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned")
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Sum(i => i.ActualRevenue ?? 0m),
            })
            .OrderByDescending(x => x.Value)
            .ToListAsync();

        return Ok(new DashboardManagementDto
        {
            IsVisible = true,
            TopSalespeople = topSalespeople,
            TeamPerformance = teamPerformanceRows,
            LeadConversionTrend = leadConversionTrend,
            RevenueByTeam = revenueByTeam,
        });
    }

    [HttpGet("activity-feed")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardActivityFeedDto>> GetActivityFeed()
    {
        var convertedLeads = await _dbContext.Leads
            .Where(x => x.ConvertedAt != null)
            .OrderByDescending(x => x.ConvertedAt)
            .Take(20)
            .Select(x => new DashboardActivityFeedItemDto
            {
                Id = x.Id,
                UserName = x.ConvertedBy != null ? x.ConvertedBy.Email ?? "System" : "System",
                Action = "Lead converted to Opportunity",
                Entity = x.Topic,
                Timestamp = x.ConvertedAt!.Value,
                Route = "/leads",
            })
            .ToListAsync();

        var approvedQuotes = await _dbContext.Quotes
            .Where(x => x.ApprovedAt != null)
            .OrderByDescending(x => x.ApprovedAt)
            .Take(20)
            .Select(x => new DashboardActivityFeedItemDto
            {
                Id = x.Id,
                UserName = x.ApprovedBy != null ? x.ApprovedBy.Email ?? "System" : "System",
                Action = "Quote approved",
                Entity = x.QuoteNumber,
                Timestamp = x.ApprovedAt!.Value,
                Route = "/sales/quotes",
            })
            .ToListAsync();

        var generatedInvoices = await _dbContext.Invoices
            .OrderByDescending(x => x.InvoiceDate ?? x.CreatedAt)
            .Take(20)
            .Select(x => new DashboardActivityFeedItemDto
            {
                Id = x.Id,
                UserName = x.OwnerUser != null ? x.OwnerUser.Email ?? "System" : "System",
                Action = "Invoice generated",
                Entity = x.InvoiceNumber,
                Timestamp = x.InvoiceDate ?? x.CreatedAt,
                Route = "/sales/invoices",
            })
            .ToListAsync();

        var escalatedCases = await _dbContext.ServiceCases
            .Where(x => x.EscalatedToUserId != null)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(20)
            .Select(x => new DashboardActivityFeedItemDto
            {
                Id = x.Id,
                UserName = x.EscalatedToUser != null ? x.EscalatedToUser.Email ?? "System" : "System",
                Action = "Case escalated",
                Entity = x.CaseNumber,
                Timestamp = x.UpdatedAt ?? x.CreatedAt,
                Route = "/service/cases",
            })
            .ToListAsync();

        var items = convertedLeads
            .Concat(approvedQuotes)
            .Concat(generatedInvoices)
            .Concat(escalatedCases)
            .OrderByDescending(x => x.Timestamp)
            .Take(40)
            .ToList();

        return Ok(new DashboardActivityFeedDto
        {
            Items = items,
        });
    }

    [HttpGet("layout-preferences")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardLayoutPreferenceDto>> GetLayoutPreferences()
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var userId = _currentUserContext.UserId.Value;
        var key = BuildLayoutPreferenceKey(userId);

        var setting = await _dbContext.SystemSettings
            .FirstOrDefaultAsync(x => x.Category == "DashboardLayout" && x.Key == key);

        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return Ok(new DashboardLayoutPreferenceDto());
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<DashboardLayoutPreferenceDto>(setting.Value);
            return Ok(parsed ?? new DashboardLayoutPreferenceDto());
        }
        catch
        {
            return Ok(new DashboardLayoutPreferenceDto());
        }
    }

    [HttpPut("layout-preferences")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<DashboardLayoutPreferenceDto>> SaveLayoutPreferences(DashboardLayoutPreferenceDto dto)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var userId = _currentUserContext.UserId.Value;
        var key = BuildLayoutPreferenceKey(userId);

        var setting = await _dbContext.SystemSettings
            .FirstOrDefaultAsync(x => x.Category == "DashboardLayout" && x.Key == key);

        var payload = JsonSerializer.Serialize(dto);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Category = "DashboardLayout",
                Key = key,
                Value = payload,
                DataType = SettingDataType.Json,
                Description = "User dashboard layout preferences",
            };

            _dbContext.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = payload;
            setting.DataType = SettingDataType.Json;
            setting.Description = "User dashboard layout preferences";
        }

        await _dbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpGet("charts/leads-by-status")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<IReadOnlyCollection<DashboardChartPointDto>>> GetLeadsByStatus()
    {
        var data = await _dbContext.Leads
            .GroupBy(x => x.LeadStatus.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("charts/opportunities-by-stage")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<IReadOnlyCollection<DashboardChartPointDto>>> GetOpportunitiesByStage()
    {
        var data = await _dbContext.Opportunities
            .Where(x => x.IsActive && !ClosedOpportunityStatusCodes.Contains(x.OpportunityStatus.Code))
            .GroupBy(x => x.OpportunityStage.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Sum(i => i.EstimatedRevenue ?? 0m),
            })
            .OrderByDescending(x => x.Value)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("charts/revenue-forecast")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<IReadOnlyCollection<DashboardChartPointDto>>> GetRevenueForecastTrend()
    {
        var data = await _dbContext.RevenueForecasts
            .OrderByDescending(x => x.ForecastDate)
            .Take(12)
            .OrderBy(x => x.ForecastDate)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.ForecastDate.ToString("yyyy-MM"),
                Count = 1,
                Value = x.ForecastRevenue,
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("charts/cases-by-priority")]
    [HasPermission("Dashboard.View")]
    public async Task<ActionResult<IReadOnlyCollection<DashboardChartPointDto>>> GetCasesByPriority()
    {
        var data = await _dbContext.ServiceCases
            .Where(x => x.IsActive && x.CaseStatus.Code != "CLOSED")
            .GroupBy(x => x.Priority.Name)
            .Select(x => new DashboardChartPointDto
            {
                Name = x.Key,
                Count = x.Count(),
                Value = x.Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return Ok(data);
    }

    private static DashboardKpiDto BuildKpi(string key, string icon, string title, decimal current, decimal previous, string actionPath, bool positiveTrendIsGood = true)
    {
        return new DashboardKpiDto
        {
            Key = key,
            Icon = icon,
            Title = title,
            CurrentValue = current,
            PreviousValue = previous,
            TrendPercent = CalculateTrendPercent(current, previous),
            ComparisonLabel = "vs previous period",
            ActionPath = actionPath,
            PositiveTrendIsGood = positiveTrendIsGood,
        };
    }

    private static decimal CalculateWinRate(int won, int lost)
    {
        var totalClosed = won + lost;
        if (totalClosed == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)won * 100m / totalClosed, 1);
    }

    private static decimal CalculateTrendPercent(decimal current, decimal previous)
    {
        if (previous == 0m)
        {
            return current == 0m ? 0m : 100m;
        }

        return Math.Round(((current - previous) / previous) * 100m, 1);
    }

    private static decimal CalculateForecastAccuracy(decimal actual, decimal forecast)
    {
        if (actual <= 0m && forecast <= 0m)
        {
            return 100m;
        }

        if (actual <= 0m)
        {
            return 0m;
        }

        var error = Math.Abs(forecast - actual) / actual;
        return Math.Max(0m, Math.Round((1m - error) * 100m, 1));
    }

    private static DateTime StartOfMonth(DateTime value)
    {
        return new DateTime(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime StartOfQuarter(DateTime value)
    {
        var quarter = (value.Month - 1) / 3;
        return new DateTime(value.Year, quarter * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private static string BuildRelatedRecord(string? accountName, string? contactName, string? leadTopic, string? opportunityTopic, string? caseNumber)
    {
        if (!string.IsNullOrWhiteSpace(opportunityTopic)) return opportunityTopic;
        if (!string.IsNullOrWhiteSpace(leadTopic)) return leadTopic;
        if (!string.IsNullOrWhiteSpace(caseNumber)) return caseNumber;
        if (!string.IsNullOrWhiteSpace(accountName)) return accountName;
        if (!string.IsNullOrWhiteSpace(contactName)) return contactName;
        return "General";
    }

    private static string BuildLayoutPreferenceKey(Guid userId)
    {
        return $"user:{userId:N}";
    }

    private async Task<(string UserName, string Role, string BusinessUnit, string Team, int OpenTasks, int OverdueActivities, int OpportunitiesClosingThisWeek, int SlaBreaches, bool HasManagementAccess)> BuildUserContextAsync(DateTime now)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return ("User", "Contributor", "Unassigned", "Unassigned", 0, 0, 0, 0, false);
        }

        var userId = _currentUserContext.UserId.Value;

        var user = await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => new { x.FirstName, x.LastName, x.Email })
            .FirstOrDefaultAsync();

        var roleNames = await (
            from ur in _dbContext.UserRoles
            join role in _dbContext.Roles on ur.RoleId equals role.Id
            where ur.UserId == userId
            select role.Name ?? string.Empty)
            .ToListAsync();

        var teamName = await _dbContext.UserTeams
            .Where(x => x.UserId == userId)
            .Select(x => x.Team.Name)
            .FirstOrDefaultAsync();

        var businessUnit = await _dbContext.UserDepartments
            .Where(x => x.UserId == userId)
            .Select(x => x.Department.Name)
            .FirstOrDefaultAsync();

        var openTasks = await _dbContext.Activities
            .CountAsync(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && !CompletedActivityStatusCodes.Contains(x.Status.Code));

        var overdueActivities = await _dbContext.Activities
            .CountAsync(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && x.DueDate != null && x.DueDate < now && !CompletedActivityStatusCodes.Contains(x.Status.Code));

        var opportunitiesClosingThisWeek = await _dbContext.Opportunities
            .CountAsync(x => x.OwnerUserId == userId && x.EstimatedCloseDate != null && x.EstimatedCloseDate >= now && x.EstimatedCloseDate <= now.AddDays(7));

        var slaBreaches = await _dbContext.ServiceCases
            .CountAsync(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && x.DueAt != null && x.DueAt < now && !ClosedCaseStatusCodes.Contains(x.CaseStatus.Code));

        var hasManagementAccess = roleNames.Any(role =>
            role.Contains("Administrator", StringComparison.OrdinalIgnoreCase)
            || role.Contains("Manager", StringComparison.OrdinalIgnoreCase));

        var roleName = roleNames
            .OrderByDescending(x => x.Contains("Administrator", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => x.Contains("Manager", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var firstName = string.IsNullOrWhiteSpace(user?.FirstName) ? null : user!.FirstName;
        var lastName = string.IsNullOrWhiteSpace(user?.LastName) ? null : user!.LastName;

        var userName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = user?.Email ?? "User";
        }

        return (
            userName,
            string.IsNullOrWhiteSpace(roleName) ? "Contributor" : roleName,
            string.IsNullOrWhiteSpace(businessUnit) ? "Unassigned" : businessUnit,
            string.IsNullOrWhiteSpace(teamName) ? "Unassigned" : teamName,
            openTasks,
            overdueActivities,
            opportunitiesClosingThisWeek,
            slaBreaches,
            hasManagementAccess);
    }
}
