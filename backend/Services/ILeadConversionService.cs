using backend.DTOs;

namespace backend.Services;

public interface ILeadConversionService
{
    Task<LeadConversionResultDto?> ConvertAsync(Guid leadId, LeadConversionRequestDto request, CancellationToken cancellationToken = default);
}
