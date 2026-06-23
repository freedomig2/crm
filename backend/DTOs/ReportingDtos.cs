namespace backend.DTOs;

public class ReportingDashboardSummaryDto
{
    public int TotalLeads { get; set; }
    public int OpenOpportunities { get; set; }
    public int TotalAccounts { get; set; }
    public int OpenCases { get; set; }
    public int OpenActivities { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal WeightedPipelineValue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal WinRate { get; set; }
}

public class ReportLibraryItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public bool IsImplemented { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public class KpiMonitoringItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal AchievementPercent { get; set; }
    public string Trend { get; set; } = string.Empty;
}
