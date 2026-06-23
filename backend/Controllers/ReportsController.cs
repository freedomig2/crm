using backend.Authorization;
using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ReportsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("dashboards")]
    [HasPermission("Reports.View")]
    public async Task<ActionResult<ReportingDashboardSummaryDto>> GetDashboardsSummary()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalLeads = await _dbContext.Leads.CountAsync();

        var openOpportunities = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED")
            .ToListAsync();

        var totalAccounts = await _dbContext.Accounts.CountAsync();

        var openCases = await _dbContext.ServiceCases
            .CountAsync(x => x.CaseStatus.Code != "CLOSED");

        var openActivities = await _dbContext.Activities
            .CountAsync(x => x.Status.Code != "COMPLETED" && x.Status.Code != "CANCELLED");

        var wonCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "WON");
        var lostCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "LOST");
        var closedCount = wonCount + lostCount;

        var revenueThisMonth = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate.HasValue && x.ActualCloseDate >= monthStart)
            .SumAsync(x => x.ActualRevenue ?? 0m);

        return Ok(new ReportingDashboardSummaryDto
        {
            TotalLeads = totalLeads,
            OpenOpportunities = openOpportunities.Count,
            TotalAccounts = totalAccounts,
            OpenCases = openCases,
            OpenActivities = openActivities,
            PipelineValue = openOpportunities.Sum(x => x.EstimatedRevenue ?? 0m),
            WeightedPipelineValue = openOpportunities.Sum(x => x.WeightedRevenue ?? 0m),
            RevenueThisMonth = revenueThisMonth,
            WinRate = closedCount == 0 ? 0m : Math.Round((decimal)wonCount / closedCount * 100m, 2),
        });
    }

    [HttpGet("library")]
    [HasPermission("Reports.View")]
    public ActionResult<IReadOnlyCollection<ReportLibraryItemDto>> GetReportLibrary()
    {
        var now = DateTime.UtcNow;
        var items = new List<ReportLibraryItemDto>
        {
            new() { Key = "dashboards", Name = "Executive Dashboards", Category = "Dashboard", Description = "Cross-module executive metrics for CRM performance.", Route = "/reporting/dashboards", IsImplemented = true, LastUpdatedAt = now },
            new() { Key = "sales-performance", Name = "Sales Performance", Category = "Analytics", Description = "Pipeline, revenue, and conversion analytics.", Route = "/reporting/sales-analytics", IsImplemented = true, LastUpdatedAt = now },
            new() { Key = "kpi-monitoring", Name = "KPI Monitoring", Category = "KPI", Description = "Target vs actual KPI tracking.", Route = "/reporting/kpi-monitoring", IsImplemented = true, LastUpdatedAt = now },
            new() { Key = "customer-analytics", Name = "Customer Analytics", Category = "Analytics", Description = "Customer behavior and segmentation analytics.", Route = "/reporting/customer-analytics", IsImplemented = false, LastUpdatedAt = now },
            new() { Key = "activity-analytics", Name = "Activity Analytics", Category = "Analytics", Description = "Activity throughput and SLA analysis.", Route = "/reporting/activity-analytics", IsImplemented = false, LastUpdatedAt = now },
            new() { Key = "service-analytics", Name = "Service Analytics", Category = "Analytics", Description = "Case resolution and service efficiency analytics.", Route = "/reporting/service-analytics", IsImplemented = false, LastUpdatedAt = now },
            new() { Key = "scheduled-reports", Name = "Scheduled Reports", Category = "Automation", Description = "Schedule periodic report generation and delivery.", Route = "/reporting/scheduled-reports", IsImplemented = false, LastUpdatedAt = now },
        };

        return Ok(items);
    }

    [HttpGet("kpis")]
    [HasPermission("Reports.View")]
    public async Task<ActionResult<IReadOnlyCollection<KpiMonitoringItemDto>>> GetKpiMonitoring()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var wonThisMonth = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate.HasValue && x.ActualCloseDate >= monthStart)
            .ToListAsync();

        var openCases = await _dbContext.ServiceCases.CountAsync(x => x.CaseStatus.Code != "CLOSED");
        var openActivities = await _dbContext.Activities.CountAsync(x => x.Status.Code != "COMPLETED" && x.Status.Code != "CANCELLED");
        var activeCustomers = await _dbContext.Accounts.CountAsync(x => x.IsActive);

        var winCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "WON");
        var lostCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "LOST");
        var totalClosed = winCount + lostCount;
        var winRate = totalClosed == 0 ? 0m : Math.Round((decimal)winCount / totalClosed * 100m, 2);

        var revenueThisMonth = wonThisMonth.Sum(x => x.ActualRevenue ?? 0m);

        List<KpiMonitoringItemDto> items =
        [
            BuildKpi("monthly-revenue", "Monthly Revenue", revenueThisMonth, 250000m, "$", "up"),
            BuildKpi("win-rate", "Win Rate", winRate, 45m, "%", "up"),
            BuildKpi("open-cases", "Open Cases", openCases, 120m, "count", "down"),
            BuildKpi("open-activities", "Open Activities", openActivities, 300m, "count", "down"),
            BuildKpi("active-customers", "Active Customers", activeCustomers, 500m, "count", "up"),
        ];

        return Ok(items);
    }

    private static KpiMonitoringItemDto BuildKpi(string key, string name, decimal current, decimal target, string unit, string trend)
    {
        var achievement = target == 0 ? 0 : Math.Round(current / target * 100m, 2);
        return new KpiMonitoringItemDto
        {
            Key = key,
            Name = name,
            CurrentValue = current,
            TargetValue = target,
            Unit = unit,
            AchievementPercent = achievement,
            Trend = trend,
        };
    }
}
