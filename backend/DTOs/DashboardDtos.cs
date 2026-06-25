namespace backend.DTOs;

public class DashboardWelcomeDto
{
    public string UserName { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = "Contributor";
    public string BusinessUnit { get; set; } = "Unassigned";
    public string Team { get; set; } = "Unassigned";
    public int OpenTasks { get; set; }
    public int OverdueActivities { get; set; }
    public int OpportunitiesClosingThisWeek { get; set; }
    public int SlaBreaches { get; set; }
    public bool HasManagementAccess { get; set; }
}

public class DashboardKpiDto
{
    public string Key { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal TrendPercent { get; set; }
    public string ComparisonLabel { get; set; } = string.Empty;
    public string ActionPath { get; set; } = string.Empty;
    public bool PositiveTrendIsGood { get; set; } = true;
}

public class DashboardSummaryDto
{
    public DashboardWelcomeDto Welcome { get; set; } = new();
    public IReadOnlyCollection<DashboardKpiDto> Kpis { get; set; } = Array.Empty<DashboardKpiDto>();
    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int QualifiedLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public int OpenOpportunities { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal WeightedPipeline { get; set; }
    public decimal WinRate { get; set; }
    public int OpenCases { get; set; }
    public int OverdueTasks { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int SlaBreaches { get; set; }
    public IReadOnlyCollection<DashboardLeadListItemDto> RecentLeads { get; set; } = Array.Empty<DashboardLeadListItemDto>();
    public IReadOnlyCollection<DashboardOpportunityListItemDto> OpportunitiesClosingSoon { get; set; } = Array.Empty<DashboardOpportunityListItemDto>();
    public IReadOnlyCollection<DashboardTaskItemDto> UpcomingFollowUps { get; set; } = Array.Empty<DashboardTaskItemDto>();
    public IReadOnlyCollection<DashboardSlaAlertItemDto> SlaAlerts { get; set; } = Array.Empty<DashboardSlaAlertItemDto>();
}

public class DashboardPipelineDto
{
    public IReadOnlyCollection<DashboardChartPointDto> FunnelStages { get; set; } = Array.Empty<DashboardChartPointDto>();
    public IReadOnlyCollection<DashboardChartPointDto> OpportunityStageDistribution { get; set; } = Array.Empty<DashboardChartPointDto>();
    public decimal ForecastAccuracyPercent { get; set; }
}

public class DashboardRevenueTrendPointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal ActualRevenue { get; set; }
    public decimal ForecastRevenue { get; set; }
}

public class DashboardRevenueDto
{
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisQuarter { get; set; }
    public IReadOnlyCollection<DashboardRevenueTrendPointDto> MonthlyTrend { get; set; } = Array.Empty<DashboardRevenueTrendPointDto>();
}

public class DashboardCustomerInsightItemDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OpenOpportunities { get; set; }
    public string? Reason { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class DashboardCustomersDto
{
    public IReadOnlyCollection<DashboardCustomerInsightItemDto> TopCustomers { get; set; } = Array.Empty<DashboardCustomerInsightItemDto>();
    public IReadOnlyCollection<DashboardCustomerInsightItemDto> AtRiskCustomers { get; set; } = Array.Empty<DashboardCustomerInsightItemDto>();
    public IReadOnlyCollection<DashboardCustomerInsightItemDto> NewCustomers { get; set; } = Array.Empty<DashboardCustomerInsightItemDto>();
}

public class DashboardServiceDto
{
    public IReadOnlyCollection<DashboardChartPointDto> CasesByPriority { get; set; } = Array.Empty<DashboardChartPointDto>();
    public IReadOnlyCollection<DashboardChartPointDto> CasesByStatus { get; set; } = Array.Empty<DashboardChartPointDto>();
    public decimal SlaCompliancePercent { get; set; }
    public IReadOnlyCollection<DashboardCaseListItemDto> CasesRequiringAttention { get; set; } = Array.Empty<DashboardCaseListItemDto>();
}

public class DashboardSalespersonPerformanceDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OpportunitiesWon { get; set; }
    public decimal WinRate { get; set; }
}

public class DashboardTeamPerformanceDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public decimal Actual { get; set; }
    public decimal AchievementPercent { get; set; }
}

public class DashboardManagementDto
{
    public bool IsVisible { get; set; }
    public IReadOnlyCollection<DashboardSalespersonPerformanceDto> TopSalespeople { get; set; } = Array.Empty<DashboardSalespersonPerformanceDto>();
    public IReadOnlyCollection<DashboardTeamPerformanceDto> TeamPerformance { get; set; } = Array.Empty<DashboardTeamPerformanceDto>();
    public IReadOnlyCollection<DashboardChartPointDto> LeadConversionTrend { get; set; } = Array.Empty<DashboardChartPointDto>();
    public IReadOnlyCollection<DashboardChartPointDto> RevenueByTeam { get; set; } = Array.Empty<DashboardChartPointDto>();
}

public class DashboardActivityFeedItemDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Route { get; set; } = string.Empty;
}

public class DashboardActivityFeedDto
{
    public IReadOnlyCollection<DashboardActivityFeedItemDto> Items { get; set; } = Array.Empty<DashboardActivityFeedItemDto>();
}

public class DashboardWidgetPreferenceDto
{
    public string WidgetId { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsPinned { get; set; }
}

public class DashboardLayoutPreferenceDto
{
    public string LayoutVersion { get; set; } = "v1";
    public IReadOnlyCollection<DashboardWidgetPreferenceDto> Widgets { get; set; } = Array.Empty<DashboardWidgetPreferenceDto>();
}

public class DashboardMyWorkDto
{
    public IReadOnlyCollection<DashboardLeadListItemDto> AssignedLeads { get; set; } = Array.Empty<DashboardLeadListItemDto>();
    public IReadOnlyCollection<DashboardOpportunityListItemDto> AssignedOpportunities { get; set; } = Array.Empty<DashboardOpportunityListItemDto>();
    public IReadOnlyCollection<DashboardCaseListItemDto> AssignedCases { get; set; } = Array.Empty<DashboardCaseListItemDto>();
    public IReadOnlyCollection<DashboardActivityItemDto> AssignedActivities { get; set; } = Array.Empty<DashboardActivityItemDto>();
    public IReadOnlyCollection<DashboardApprovalItemDto> PendingApprovals { get; set; } = Array.Empty<DashboardApprovalItemDto>();
    public IReadOnlyCollection<DashboardTaskItemDto> OverdueTasks { get; set; } = Array.Empty<DashboardTaskItemDto>();
}

public class DashboardMyActivitiesFilterDto : ListQueryDto
{
    public Guid? ActivityTypeId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? PriorityId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? RelatedRecord { get; set; }
}

public class DashboardMyActivitiesDto
{
    public IReadOnlyCollection<DashboardActivityItemDto> Items { get; set; } = Array.Empty<DashboardActivityItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IReadOnlyCollection<DashboardChartPointDto> ActivitiesByStatus { get; set; } = Array.Empty<DashboardChartPointDto>();
}

public class DashboardMyOpenTasksFilterDto : ListQueryDto
{
    public Guid? PriorityId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}

public class DashboardMyOpenTasksDto
{
    public IReadOnlyCollection<DashboardTaskItemDto> Items { get; set; } = Array.Empty<DashboardTaskItemDto>();
    public int TotalCount { get; set; }
    public int OverdueCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class DashboardLeadListItemDto
{
    public Guid Id { get; set; }
    public string LeadNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? StatusName { get; set; }
    public int Score { get; set; }
    public string? OwnerName { get; set; }
}

public class DashboardOpportunityListItemDto
{
    public Guid Id { get; set; }
    public string OpportunityNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? AccountName { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public decimal Probability { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public string? StageName { get; set; }
}

public class DashboardCaseListItemDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? PriorityName { get; set; }
    public string? StatusName { get; set; }
    public DateTime? DueAt { get; set; }
    public string? AssignedToName { get; set; }
}

public class DashboardActivityItemDto
{
    public Guid Id { get; set; }
    public string ActivityNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? ActivityTypeName { get; set; }
    public string? StatusName { get; set; }
    public string? PriorityName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime ActivityDate { get; set; }
    public string RelatedRecord { get; set; } = string.Empty;
}

public class DashboardTaskItemDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? PriorityName { get; set; }
    public DateTime? DueDate { get; set; }
    public string RelatedRecord { get; set; } = string.Empty;
    public string? StatusName { get; set; }
    public bool IsOverdue { get; set; }
}

public class DashboardApprovalItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? AccountName { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ApprovalStatusName { get; set; }
}

public class DashboardSlaAlertItemDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? PriorityName { get; set; }
    public DateTime? DueAt { get; set; }
    public string? AssignedToName { get; set; }
}

public class DashboardChartPointDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Value { get; set; }
}