using backend.DTOs;

namespace backend.Services;

public interface IAiProvider
{
    string Name { get; }
    Task<IReadOnlyCollection<string>> RecommendActionsAsync(string scenarioCode, string? contextText, CancellationToken cancellationToken = default);
}

public interface IAiRecommendationService
{
    Task<AiRecommendationDto> RecommendActionsAsync(string scenarioCode, string? contextText, CancellationToken cancellationToken = default);
}

public class AiRecommendationService : IAiRecommendationService
{
    private readonly IAiProvider _provider;

    public AiRecommendationService(IAiProvider provider)
    {
        _provider = provider;
    }

    public async Task<AiRecommendationDto> RecommendActionsAsync(string scenarioCode, string? contextText, CancellationToken cancellationToken = default)
    {
        var normalizedScenario = scenarioCode.Trim().ToUpperInvariant();
        var actions = await _provider.RecommendActionsAsync(normalizedScenario, contextText, cancellationToken);

        return new AiRecommendationDto
        {
            ScenarioCode = normalizedScenario,
            Actions = actions,
        };
    }
}

public class RulesBasedAiProvider : IAiProvider
{
    public string Name => "RulesBasedAiProvider";

    public Task<IReadOnlyCollection<string>> RecommendActionsAsync(string scenarioCode, string? contextText, CancellationToken cancellationToken = default)
    {
        var context = (contextText ?? string.Empty).Trim();

        IReadOnlyCollection<string> actions = scenarioCode switch
        {
            "LEAD_FOLLOW_UP" => new[]
            {
                "Assign lead owner and schedule follow-up within 24 hours.",
                "Send a personalized outreach using lead source context.",
                "Set qualification checklist status after first contact.",
            },
            "OPPORTUNITY_RISK" => new[]
            {
                "Review last stage movement and update close plan.",
                "Schedule stakeholder alignment meeting.",
                "Log top 2 blockers and mitigation actions.",
            },
            "CASE_ESCALATION" => new[]
            {
                "Increase case priority and notify service manager.",
                "Post customer-facing update with target resolution time.",
                "Create linked follow-up activity for resolution validation.",
            },
            _ => new[]
            {
                "Capture additional context and run recommendation again.",
                "Create a follow-up activity with owner and due date.",
                "Update status fields to reflect latest next step.",
            }
        };

        if (!string.IsNullOrWhiteSpace(context))
        {
            actions = actions.Concat(new[] { $"Context signal: {context[..Math.Min(context.Length, 120)]}" }).ToArray();
        }

        return Task.FromResult(actions);
    }
}
