using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public interface IOpportunityConversionAdapter
{
    Task<Guid?> TryCreateOpportunityAsync(Lead lead, LeadConversionRequestDto request, Guid accountId, Guid contactId, CancellationToken cancellationToken = default);
}

public class NoOpOpportunityConversionAdapter : IOpportunityConversionAdapter
{
    public Task<Guid?> TryCreateOpportunityAsync(Lead lead, LeadConversionRequestDto request, Guid accountId, Guid contactId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Guid?>(null);
    }
}
