using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SalesPipelineFilterDto
{
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public Guid? StageId { get; set; }
    public decimal? MinRevenue { get; set; }
    public decimal? MaxRevenue { get; set; }
    public decimal? MinProbability { get; set; }
    public decimal? MaxProbability { get; set; }
    public DateTime? CloseDateFrom { get; set; }
    public DateTime? CloseDateTo { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? RatingId { get; set; }
    public Guid? IndustryId { get; set; }
}

public class SalesPipelineBoardDto
{
    public SalesPipelineSummaryDto Summary { get; set; } = new();
    public IReadOnlyCollection<SalesPipelineStageDto> Stages { get; set; } = Array.Empty<SalesPipelineStageDto>();
}

public class SalesPipelineSummaryDto
{
    public int TotalOpportunities { get; set; }
    public decimal PipelineRevenue { get; set; }
    public decimal WeightedPipelineRevenue { get; set; }
    public decimal AverageProbability { get; set; }
    public decimal AverageDealSize { get; set; }
}

public class SalesPipelineStageDto
{
    public Guid StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? StageCode { get; set; }
    public int Count { get; set; }
    public decimal PipelineRevenue { get; set; }
    public decimal WeightedRevenue { get; set; }
    public IReadOnlyCollection<SalesPipelineCardDto> Opportunities { get; set; } = Array.Empty<SalesPipelineCardDto>();
}

public class SalesPipelineCardDto
{
    public Guid Id { get; set; }
    public string OpportunityNumber { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? AccountName { get; set; }
    public decimal? EstimatedRevenue { get; set; }
    public decimal Probability { get; set; }
    public decimal? WeightedRevenue { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public Guid OpportunityStageId { get; set; }
    public string? OpportunityStageName { get; set; }
    public Guid? SalesProcessStageId { get; set; }
    public string? OwnerName { get; set; }
    public string? RatingName { get; set; }
    public string? RatingCode { get; set; }
    public int AgeInDays { get; set; }
}

public class PipelineMoveStageRequestDto
{
    [Required]
    public Guid OpportunityId { get; set; }

    [Required]
    public Guid NewStageId { get; set; }

    public Guid? SalesProcessStageId { get; set; }
    public string? Notes { get; set; }
}

public class OpportunityStageHistoryDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public Guid? PreviousStageId { get; set; }
    public string? PreviousStageName { get; set; }
    public Guid NewStageId { get; set; }
    public string NewStageName { get; set; } = string.Empty;
    public Guid? ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }
}

public class OpportunityPipelineAnalyticsDto
{
    public Guid OpportunityId { get; set; }
    public string? CurrentStageName { get; set; }
    public int DaysInStage { get; set; }
    public IReadOnlyCollection<OpportunityStageHistoryDto> StageHistory { get; set; } = Array.Empty<OpportunityStageHistoryDto>();
    public IReadOnlyCollection<TrendPointDto> ProbabilityTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueTrend { get; set; } = Array.Empty<TrendPointDto>();
}

public class SalesTargetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TargetTypeId { get; set; }
    public string? TargetTypeName { get; set; }
    public Guid TargetPeriodId { get; set; }
    public string? TargetPeriodName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal AchievementPercentage { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public string? AssignedTeamName { get; set; }
    public bool IsActive { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? OwnerUserName { get; set; }
    public Guid? OwnerTeamId { get; set; }
    public string? OwnerTeamName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SalesTargetFilterDto : ListQueryDto
{
    public Guid? TargetTypeId { get; set; }
    public Guid? TargetPeriodId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public bool? IsActive { get; set; }
}

public class UpsertSalesTargetRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public Guid TargetTypeId { get; set; }
    [Required]
    public Guid TargetPeriodId { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public Guid? OwnerTeamId { get; set; }
}

public class RevenueForecastDto
{
    public Guid Id { get; set; }
    public DateTime ForecastDate { get; set; }
    public DateTime ForecastPeriodStart { get; set; }
    public DateTime ForecastPeriodEnd { get; set; }
    public Guid ForecastTypeId { get; set; }
    public string? ForecastTypeName { get; set; }
    public decimal TotalPipelineRevenue { get; set; }
    public decimal WeightedPipelineRevenue { get; set; }
    public decimal ForecastRevenue { get; set; }
    public decimal ClosedRevenue { get; set; }
    public decimal OpenRevenue { get; set; }
    public decimal ForecastAccuracy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RevenueForecastFilterDto : ListQueryDto
{
    public Guid? ForecastTypeId { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
}

public class UpsertRevenueForecastRequestDto
{
    public DateTime ForecastDate { get; set; }
    [Required]
    public DateTime ForecastPeriodStart { get; set; }
    [Required]
    public DateTime ForecastPeriodEnd { get; set; }
    [Required]
    public Guid ForecastTypeId { get; set; }
    public decimal? ForecastRevenue { get; set; }
    public string? Notes { get; set; }
}

public class ForecastDashboardDto
{
    public decimal TotalPipeline { get; set; }
    public decimal WeightedPipeline { get; set; }
    public decimal ClosedRevenue { get; set; }
    public decimal ForecastRevenue { get; set; }
    public decimal ForecastAccuracy { get; set; }
    public IReadOnlyCollection<TrendPointDto> ForecastTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueByMonth { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueByQuarter { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueByOwner { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueByTeam { get; set; } = Array.Empty<TrendPointDto>();
}

public class RevenueTrackingDto
{
    public decimal WonRevenue { get; set; }
    public decimal LostRevenue { get; set; }
    public decimal PipelineRevenue { get; set; }
    public decimal WeightedRevenue { get; set; }
    public IReadOnlyCollection<TrendPointDto> RevenueTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueByAccount { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueByIndustry { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueBySalesperson { get; set; } = Array.Empty<TrendPointDto>();
}

public class SalesPerformanceDashboardDto
{
    public int OpenOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal WinRate { get; set; }
    public decimal AverageDealSize { get; set; }
    public decimal AverageSalesCycleDays { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisQuarter { get; set; }
    public decimal RevenueThisYear { get; set; }
    public decimal ForecastRevenue { get; set; }
    public decimal ForecastAccuracy { get; set; }
    public string? TopSalesperson { get; set; }
    public string? TopTeam { get; set; }
    public IReadOnlyCollection<TrendPointDto> PipelineByStage { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> RevenueTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> OpportunitiesByOwner { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> OpportunitiesByIndustry { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> WinRateTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<TrendPointDto> ForecastAccuracyTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyCollection<LeaderboardItemDto> TopSalespeople { get; set; } = Array.Empty<LeaderboardItemDto>();
    public IReadOnlyCollection<LeaderboardItemDto> TopTeams { get; set; } = Array.Empty<LeaderboardItemDto>();
}

public class LeaderboardItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal WinRate { get; set; }
    public int OpportunitiesClosed { get; set; }
    public decimal TargetAchievement { get; set; }
}

public class TrendPointDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
}
