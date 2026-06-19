using backend.DTOs;
using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

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

public class OpportunityConversionAdapter : IOpportunityConversionAdapter
{
    private readonly AppDbContext _dbContext;
    private readonly INumberSequenceService _numberSequenceService;

    public OpportunityConversionAdapter(AppDbContext dbContext, INumberSequenceService numberSequenceService)
    {
        _dbContext = dbContext;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<Guid?> TryCreateOpportunityAsync(Lead lead, LeadConversionRequestDto request, Guid accountId, Guid contactId, CancellationToken cancellationToken = default)
    {
        var stageId = await GetLookupValueIdAsync("OPPORTUNITY_STAGE", "QUALIFY", cancellationToken)
            ?? throw new InvalidOperationException("Opportunity qualify stage is not configured.");
        var statusId = await GetLookupValueIdAsync("OPPORTUNITY_STATUS", "OPEN", cancellationToken)
            ?? throw new InvalidOperationException("Opportunity open status is not configured.");
        var sourceId = await GetLookupValueIdAsync("OPPORTUNITY_SOURCE", "LEAD_CONVERSION", cancellationToken);

        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            OpportunityNumber = await _numberSequenceService.GenerateNextAsync("OPPORTUNITY"),
            Topic = FirstNonBlank(request.OpportunityTopic, lead.Topic, lead.CompanyName, lead.FullName) ?? "Converted Lead Opportunity",
            AccountId = accountId,
            ContactId = contactId,
            LeadId = lead.Id,
            OpportunityStageId = stageId,
            OpportunityStatusId = statusId,
            EstimatedRevenue = request.EstimatedValue ?? lead.EstimatedValue,
            EstimatedCloseDate = request.EstimatedCloseDate ?? lead.EstimatedCloseDate,
            Probability = 0m,
            WeightedRevenue = 0m,
            SourceId = sourceId,
            Description = lead.Description,
            Notes = lead.Notes,
            OwnerUserId = lead.OwnerUserId ?? lead.AssignedToUserId,
            OwnerTeamId = lead.OwnerTeamId ?? lead.AssignedToTeamId,
            IsActive = true
        };

        _dbContext.Opportunities.Add(opportunity);
        return opportunity.Id;
    }

    private async Task<Guid?> GetLookupValueIdAsync(string categoryCode, string valueCode, CancellationToken cancellationToken)
    {
        return await _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == categoryCode && x.Code == valueCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        return values.Select(TrimToNull).FirstOrDefault(value => value is not null);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
