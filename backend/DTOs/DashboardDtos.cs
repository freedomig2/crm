namespace backend.DTOs;

public class DashboardSummaryDto
{
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