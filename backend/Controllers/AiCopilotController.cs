using backend.Authorization;
using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/ai")]
public class AiCopilotController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAiRecommendationService _aiRecommendationService;

    public AiCopilotController(AppDbContext dbContext, IAiRecommendationService aiRecommendationService)
    {
        _dbContext = dbContext;
        _aiRecommendationService = aiRecommendationService;
    }

    [HttpGet("dashboard")]
    [HasPermission("AI.View")]
    public async Task<ActionResult<AiDashboardSummaryDto>> GetDashboardSummary()
    {
        var openLeads = await _dbContext.Leads.CountAsync(x => x.LeadStatus.Code != "CONVERTED" && x.LeadStatus.Code != "DISQUALIFIED");
        var openOpportunities = await _dbContext.Opportunities.CountAsync(x => x.OpportunityStatus.Code == "OPEN" || x.OpportunityStatus.Code == "ON_HOLD");
        var openCases = await _dbContext.ServiceCases.CountAsync(x => x.CaseStatus.Code != "CLOSED");

        var insights = new List<string>();
        if (openLeads > 0)
        {
            insights.Add($"{openLeads} leads require active follow-up.");
        }

        if (openOpportunities > 0)
        {
            insights.Add($"{openOpportunities} opportunities are still open in pipeline.");
        }

        if (openCases > 0)
        {
            insights.Add($"{openCases} service cases are not yet closed.");
        }

        if (insights.Count == 0)
        {
            insights.Add("No immediate operational risk signals detected.");
        }

        return Ok(new AiDashboardSummaryDto
        {
            OpenLeads = openLeads,
            OpenOpportunities = openOpportunities,
            OpenCases = openCases,
            Insights = insights,
        });
    }

    [HttpPost("recommendations")]
    [HasPermission("AI.View")]
    public async Task<ActionResult<AiRecommendationDto>> GetRecommendations(AiRecommendationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ScenarioCode))
        {
            return BadRequest("Scenario code is required.");
        }

        var response = await _aiRecommendationService.RecommendActionsAsync(request.ScenarioCode, request.ContextText);
        return Ok(response);
    }
}
