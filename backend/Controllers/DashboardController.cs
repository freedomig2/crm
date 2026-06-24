using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
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
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var wonCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "WON");
        var lostCount = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "LOST");

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
            .Where(x => x.IsActive && x.DueDate != null && x.DueDate >= now && x.DueDate <= now.AddDays(7) && x.Status.Code != "COMPLETED")
            .OrderBy(x => x.DueDate)
            .Take(8)
            .Select(x => new DashboardTaskItemDto
            {
                Id = x.Id,
                Subject = x.Subject,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                DueDate = x.DueDate,
                StatusName = x.Status.Name,
                RelatedRecord = BuildRelatedRecord(x.Account != null ? x.Account.Name : null, x.Contact != null ? x.Contact.FullName : null, x.Lead != null ? x.Lead.Topic : null, x.Opportunity != null ? x.Opportunity.Topic : null, x.Case != null ? x.Case.CaseNumber : null),
                IsOverdue = false,
            })
            .ToListAsync();

        var slaAlerts = await _dbContext.ServiceCases
            .Where(x => x.IsActive && x.DueAt != null && x.DueAt < now && x.CaseStatus.Code != "CLOSED" && x.CaseStatus.Code != "RESOLVED")
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

        var result = new DashboardSummaryDto
        {
            TotalLeads = await _dbContext.Leads.CountAsync(),
            NewLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "NEW"),
            QualifiedLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code == "QUALIFIED" || (x.QualificationStatus != null && x.QualificationStatus.Code == "QUALIFIED")),
            ConvertedLeads = await _dbContext.Leads.CountAsync(x => x.ConvertedAt != null || x.LeadStatus.Code == "CONVERTED"),
            OpenOpportunities = await _dbContext.Opportunities.CountAsync(x => x.IsActive && x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED"),
            PipelineValue = await _dbContext.Opportunities.Where(x => x.IsActive && x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED").SumAsync(x => x.EstimatedRevenue ?? 0m),
            WeightedPipeline = await _dbContext.Opportunities.Where(x => x.IsActive && x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED").SumAsync(x => x.WeightedRevenue ?? 0m),
            WinRate = CalculateWinRate(wonCount, lostCount),
            OpenCases = await _dbContext.ServiceCases.CountAsync(x => x.IsActive && x.CaseStatus.Code != "CLOSED" && x.CaseStatus.Code != "RESOLVED"),
            OverdueTasks = await _dbContext.Activities.CountAsync(x => x.IsActive && x.DueDate != null && x.DueDate < now && x.Status.Code != "COMPLETED"),
            RevenueThisMonth = await _dbContext.Opportunities.Where(x => x.OpportunityStatus.Code == "WON" && x.ActualCloseDate != null && x.ActualCloseDate >= monthStart).SumAsync(x => x.ActualRevenue ?? 0m),
            SlaBreaches = await _dbContext.ServiceCases.CountAsync(x => x.IsActive && x.DueAt != null && x.DueAt < now && x.CaseStatus.Code != "CLOSED" && x.CaseStatus.Code != "RESOLVED"),
            RecentLeads = await _dbContext.Leads
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
                .ToListAsync(),
            OpportunitiesClosingSoon = opportunitiesClosingSoon,
            UpcomingFollowUps = upcomingFollowUps,
            SlaAlerts = slaAlerts,
        };

        return Ok(result);
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
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
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
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
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
            .Take(12)
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
                RelatedRecord = BuildRelatedRecord(x.Account != null ? x.Account.Name : null, x.Contact != null ? x.Contact.FullName : null, x.Lead != null ? x.Lead.Topic : null, x.Opportunity != null ? x.Opportunity.Topic : null, x.Case != null ? x.Case.CaseNumber : null),
            })
            .ToListAsync();

        var pendingApprovals = await _dbContext.Quotes
            .Where(x => x.ApprovedById == userId || (x.ApprovalStatus.Code != "APPROVED" && x.OwnerUserId == userId))
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(8)
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
            .Where(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && x.DueDate != null && x.DueDate < now && x.Status.Code != "COMPLETED")
            .OrderBy(x => x.DueDate)
            .Take(10)
            .Select(x => new DashboardTaskItemDto
            {
                Id = x.Id,
                Subject = x.Subject,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                DueDate = x.DueDate,
                StatusName = x.Status.Name,
                RelatedRecord = BuildRelatedRecord(x.Account != null ? x.Account.Name : null, x.Contact != null ? x.Contact.FullName : null, x.Lead != null ? x.Lead.Topic : null, x.Opportunity != null ? x.Opportunity.Topic : null, x.Case != null ? x.Case.CaseNumber : null),
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
            RelatedRecord = BuildRelatedRecord(x.Account != null ? x.Account.Name : null, x.Contact != null ? x.Contact.FullName : null, x.Lead != null ? x.Lead.Topic : null, x.Opportunity != null ? x.Opportunity.Topic : null, x.Case != null ? x.Case.CaseNumber : null),
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
            .Where(x => (x.AssignedToUserId == userId || x.OwnerUserId == userId) && x.Status.Code != "COMPLETED");

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
                RelatedRecord = BuildRelatedRecord(x.Account != null ? x.Account.Name : null, x.Contact != null ? x.Contact.FullName : null, x.Lead != null ? x.Lead.Topic : null, x.Opportunity != null ? x.Opportunity.Topic : null, x.Case != null ? x.Case.CaseNumber : null),
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
            .Where(x => x.IsActive && x.OpportunityStatus.Code != "WON" && x.OpportunityStatus.Code != "LOST" && x.OpportunityStatus.Code != "CANCELLED")
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

    private static decimal CalculateWinRate(int won, int lost)
    {
        var totalClosed = won + lost;
        if (totalClosed == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)won * 100m / totalClosed, 1);
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
}