using CRM.Domain.Common;

namespace backend.Entities;

public class RevenueForecast : BaseEntity
{
    public DateTime ForecastDate { get; set; }
    public DateTime ForecastPeriodStart { get; set; }
    public DateTime ForecastPeriodEnd { get; set; }
    public Guid ForecastTypeId { get; set; }
    public LookupValue ForecastType { get; set; } = default!;
    public decimal TotalPipelineRevenue { get; set; }
    public decimal WeightedPipelineRevenue { get; set; }
    public decimal ForecastRevenue { get; set; }
    public decimal ClosedRevenue { get; set; }
    public decimal OpenRevenue { get; set; }
    public string? Notes { get; set; }
}
