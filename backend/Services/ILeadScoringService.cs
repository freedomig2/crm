using backend.Entities;

namespace backend.Services;

public interface ILeadScoringService
{
    Task ApplyScoreAsync(Lead lead, CancellationToken cancellationToken = default);
    Task<int> RecalculateLeadScoreAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task<int> RecalculateAllLeadScoresAsync(CancellationToken cancellationToken = default);
}
