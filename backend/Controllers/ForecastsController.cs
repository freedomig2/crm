using System.Globalization;
using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/forecasts")]
public class ForecastsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ForecastsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HasPermission("Forecasts.View")]
    public async Task<ActionResult<PagedResult<RevenueForecastDto>>> GetForecasts([FromQuery] RevenueForecastFilterDto query)
    {
        var forecasts = _dbContext.RevenueForecasts.AsQueryable();

        if (query.ForecastTypeId.HasValue)
        {
            forecasts = forecasts.Where(x => x.ForecastTypeId == query.ForecastTypeId.Value);
        }

        if (query.PeriodFrom.HasValue)
        {
            forecasts = forecasts.Where(x => x.ForecastPeriodEnd >= query.PeriodFrom.Value);
        }

        if (query.PeriodTo.HasValue)
        {
            forecasts = forecasts.Where(x => x.ForecastPeriodStart <= query.PeriodTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            forecasts = forecasts.Where(x =>
                (x.Notes ?? string.Empty).ToLower().Contains(search) ||
                x.ForecastType.Name.ToLower().Contains(search));
        }

        forecasts = forecasts.OrderByPropertyName(query.SortBy, query.SortDir);
        if (string.IsNullOrWhiteSpace(query.SortBy))
        {
            forecasts = forecasts.OrderByDescending(x => x.ForecastDate);
        }

        return Ok(await ProjectForecasts(forecasts).ToPagedAsync(query));
    }

    [HttpGet("dashboard")]
    [HasPermission("Forecasts.View")]
    public async Task<ActionResult<ForecastDashboardDto>> GetDashboard()
    {
        var latest = await _dbContext.RevenueForecasts.OrderByDescending(x => x.ForecastDate).FirstOrDefaultAsync();
        var open = _dbContext.Opportunities.Where(x => x.OpportunityStatus.Code == "OPEN" || x.OpportunityStatus.Code == "ON_HOLD");
        var won = _dbContext.Opportunities.Where(x => x.OpportunityStatus.Code == "WON");
        var forecastTrend = await _dbContext.RevenueForecasts
            .OrderBy(x => x.ForecastDate)
            .Select(x => new { x.ForecastDate, x.ForecastRevenue })
            .ToListAsync();

        return Ok(new ForecastDashboardDto
        {
            TotalPipeline = await open.SumAsync(x => x.EstimatedRevenue ?? 0m),
            WeightedPipeline = await open.SumAsync(x => x.WeightedRevenue ?? 0m),
            ClosedRevenue = await won.SumAsync(x => x.ActualRevenue ?? 0m),
            ForecastRevenue = latest?.ForecastRevenue ?? 0m,
            ForecastAccuracy = CalculateForecastAccuracy(latest?.ClosedRevenue ?? 0m, latest?.ForecastRevenue ?? 0m),
            ForecastTrend = forecastTrend
                .Select(x => new TrendPointDto { Name = x.ForecastDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Value = x.ForecastRevenue, Count = 1 })
                .ToList(),
            RevenueByMonth = await BuildClosedRevenueByMonthAsync(),
            RevenueByQuarter = await BuildClosedRevenueByQuarterAsync(),
            RevenueByOwner = await BuildRevenueByOwnerAsync(),
            RevenueByTeam = await BuildRevenueByTeamAsync()
        });
    }

    [HttpGet("{id:guid}")]
    [HasPermission("Forecasts.View")]
    public async Task<ActionResult<RevenueForecastDto>> GetForecast(Guid id)
    {
        var forecast = await ProjectForecasts(_dbContext.RevenueForecasts.Where(x => x.Id == id)).FirstOrDefaultAsync();
        return forecast is null ? NotFound() : Ok(forecast);
    }

    [HttpPost]
    [HasPermission("Forecasts.Create")]
    public async Task<ActionResult<RevenueForecastDto>> CreateForecast(UpsertRevenueForecastRequestDto dto)
    {
        var validationError = await ValidateForecastAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var forecast = new RevenueForecast();
        await ApplyForecastValuesAsync(forecast, dto);
        _dbContext.RevenueForecasts.Add(forecast);
        await _dbContext.SaveChangesAsync();

        var created = await ProjectForecasts(_dbContext.RevenueForecasts.Where(x => x.Id == forecast.Id)).FirstOrDefaultAsync();
        return created is null ? Problem("Forecast was created but could not be loaded.") : Ok(created);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("Forecasts.Update")]
    public async Task<IActionResult> UpdateForecast(Guid id, UpsertRevenueForecastRequestDto dto)
    {
        var forecast = await _dbContext.RevenueForecasts.FirstOrDefaultAsync(x => x.Id == id);
        if (forecast is null)
        {
            return NotFound();
        }

        var validationError = await ValidateForecastAsync(dto);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        await ApplyForecastValuesAsync(forecast, dto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("Forecasts.Delete")]
    public async Task<IActionResult> DeleteForecast(Guid id)
    {
        var forecast = await _dbContext.RevenueForecasts.FirstOrDefaultAsync(x => x.Id == id);
        if (forecast is null)
        {
            return NotFound();
        }

        forecast.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateForecastAsync(UpsertRevenueForecastRequestDto dto)
    {
        if (dto.ForecastPeriodEnd < dto.ForecastPeriodStart)
        {
            return "Forecast period end must be on or after the start date.";
        }

        if (!await _dbContext.LookupValues.AnyAsync(x => x.Id == dto.ForecastTypeId && x.LookupCategory.Code == "FORECAST_TYPE"))
        {
            return "Forecast type is required.";
        }

        return null;
    }

    private async Task ApplyForecastValuesAsync(RevenueForecast forecast, UpsertRevenueForecastRequestDto dto)
    {
        var calculated = await CalculateForecastValuesAsync(dto.ForecastPeriodStart, dto.ForecastPeriodEnd, dto.ForecastTypeId);
        forecast.ForecastDate = dto.ForecastDate == default ? DateTime.UtcNow : dto.ForecastDate;
        forecast.ForecastPeriodStart = dto.ForecastPeriodStart;
        forecast.ForecastPeriodEnd = dto.ForecastPeriodEnd;
        forecast.ForecastTypeId = dto.ForecastTypeId;
        forecast.TotalPipelineRevenue = calculated.TotalPipelineRevenue;
        forecast.WeightedPipelineRevenue = calculated.WeightedPipelineRevenue;
        forecast.ClosedRevenue = calculated.ClosedRevenue;
        forecast.OpenRevenue = calculated.OpenRevenue;
        forecast.ForecastRevenue = dto.ForecastRevenue ?? calculated.ForecastRevenue;
        forecast.Notes = TrimToNull(dto.Notes);
    }

    private async Task<RevenueForecast> CalculateForecastValuesAsync(DateTime periodStart, DateTime periodEnd, Guid forecastTypeId)
    {
        var forecastTypeCode = await _dbContext.LookupValues
            .Where(x => x.Id == forecastTypeId)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();
        var multiplier = forecastTypeCode switch
        {
            "CONSERVATIVE" => 0.8m,
            "AGGRESSIVE" => 1.2m,
            _ => 1m
        };

        var open = _dbContext.Opportunities
            .Where(x => (x.OpportunityStatus.Code == "OPEN" || x.OpportunityStatus.Code == "ON_HOLD") &&
                        (!x.EstimatedCloseDate.HasValue || (x.EstimatedCloseDate >= periodStart && x.EstimatedCloseDate <= periodEnd)));
        var won = _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" &&
                        x.ActualCloseDate >= periodStart && x.ActualCloseDate <= periodEnd);

        var weighted = await open.SumAsync(x => x.WeightedRevenue ?? 0m);
        return new RevenueForecast
        {
            TotalPipelineRevenue = await open.SumAsync(x => x.EstimatedRevenue ?? 0m),
            WeightedPipelineRevenue = weighted,
            ForecastRevenue = Math.Round(weighted * multiplier, 2),
            ClosedRevenue = await won.SumAsync(x => x.ActualRevenue ?? 0m),
            OpenRevenue = await open.SumAsync(x => x.EstimatedRevenue ?? 0m)
        };
    }

    private static IQueryable<RevenueForecastDto> ProjectForecasts(IQueryable<RevenueForecast> query)
    {
        return query.Select(x => new RevenueForecastDto
        {
            Id = x.Id,
            ForecastDate = x.ForecastDate,
            ForecastPeriodStart = x.ForecastPeriodStart,
            ForecastPeriodEnd = x.ForecastPeriodEnd,
            ForecastTypeId = x.ForecastTypeId,
            ForecastTypeName = x.ForecastType.Name,
            TotalPipelineRevenue = x.TotalPipelineRevenue,
            WeightedPipelineRevenue = x.WeightedPipelineRevenue,
            ForecastRevenue = x.ForecastRevenue,
            ClosedRevenue = x.ClosedRevenue,
            OpenRevenue = x.OpenRevenue,
            ForecastAccuracy = x.ForecastRevenue <= 0m ? 0m : Math.Round(x.ClosedRevenue / x.ForecastRevenue * 100m, 1),
            Notes = x.Notes,
            CreatedAt = x.CreatedAt
        });
    }

    private async Task<IReadOnlyCollection<TrendPointDto>> BuildClosedRevenueByMonthAsync()
    {
        var rows = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate.HasValue)
            .Select(x => new { Date = x.ActualCloseDate!.Value, Revenue = x.ActualRevenue ?? 0m })
            .ToListAsync();

        return rows
            .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1))
            .OrderBy(x => x.Key)
            .Select(x => new TrendPointDto { Name = x.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture), Value = x.Sum(i => i.Revenue), Count = x.Count() })
            .ToList();
    }

    private async Task<IReadOnlyCollection<TrendPointDto>> BuildClosedRevenueByQuarterAsync()
    {
        var rows = await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate.HasValue)
            .Select(x => new { Date = x.ActualCloseDate!.Value, Revenue = x.ActualRevenue ?? 0m })
            .ToListAsync();

        return rows
            .GroupBy(x => $"{x.Date.Year} Q{((x.Date.Month - 1) / 3) + 1}")
            .OrderBy(x => x.Key)
            .Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.Revenue), Count = x.Count() })
            .ToList();
    }

    private async Task<IReadOnlyCollection<TrendPointDto>> BuildRevenueByOwnerAsync()
    {
        return await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON")
            .GroupBy(x => x.OwnerUser != null ? x.OwnerUser.Email! : x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned")
            .Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.ActualRevenue ?? 0m), Count = x.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync();
    }

    private async Task<IReadOnlyCollection<TrendPointDto>> BuildRevenueByTeamAsync()
    {
        return await _dbContext.Opportunities
            .Where(x => x.OpportunityStatus.Code == "WON")
            .GroupBy(x => x.OwnerTeam != null ? x.OwnerTeam.Name : "Unassigned")
            .Select(x => new TrendPointDto { Name = x.Key, Value = x.Sum(i => i.ActualRevenue ?? 0m), Count = x.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync();
    }

    private static decimal CalculateForecastAccuracy(decimal closedRevenue, decimal forecastRevenue)
    {
        return forecastRevenue <= 0m ? 0m : Math.Round(closedRevenue / forecastRevenue * 100m, 1);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
