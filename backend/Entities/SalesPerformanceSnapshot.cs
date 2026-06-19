using CRM.Domain.Common;

namespace backend.Entities;

public class SalesPerformanceSnapshot : BaseEntity
{
    public DateTime SnapshotDate { get; set; }
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public int OpenOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal PipelineRevenue { get; set; }
    public decimal WeightedRevenue { get; set; }
    public decimal ClosedRevenue { get; set; }
    public decimal AverageDealSize { get; set; }
    public decimal AverageSalesCycleDays { get; set; }
    public decimal WinRate { get; set; }
    public decimal ForecastAccuracy { get; set; }
}
