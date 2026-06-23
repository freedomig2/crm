using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public interface IIntegrationProvider
{
    string ProviderCode { get; }
    Task<IntegrationActionResultDto> TestConnectionAsync(IntegrationConnection connection, CancellationToken cancellationToken = default);
    Task<IntegrationSyncResult> RunSyncAsync(IntegrationConnection connection, CancellationToken cancellationToken = default);
}

public sealed class IntegrationSyncResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int RecordsProcessed { get; init; }
}

public interface IIntegrationExecutionService
{
    Task<IntegrationActionResultDto> TestConnectionAsync(IntegrationConnection connection, CancellationToken cancellationToken = default);
    Task<IntegrationSyncResult> RunSyncAsync(IntegrationConnection connection, CancellationToken cancellationToken = default);
    Task<LookupValue?> GetStatusByCodeAsync(string statusCode, CancellationToken cancellationToken = default);
    Task<LookupValue?> GetTriggerTypeByCodeAsync(string triggerTypeCode, CancellationToken cancellationToken = default);
}

public class IntegrationExecutionService : IIntegrationExecutionService
{
    private readonly IEnumerable<IIntegrationProvider> _providers;
    private readonly AppDbContext _dbContext;

    public IntegrationExecutionService(IEnumerable<IIntegrationProvider> providers, AppDbContext dbContext)
    {
        _providers = providers;
        _dbContext = dbContext;
    }

    public async Task<IntegrationActionResultDto> TestConnectionAsync(IntegrationConnection connection, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(connection.Provider.Code);
        if (provider is null)
        {
            return new IntegrationActionResultDto
            {
                Success = false,
                Message = $"No provider implementation is registered for {connection.Provider.Code}."
            };
        }

        return await provider.TestConnectionAsync(connection, cancellationToken);
    }

    public async Task<IntegrationSyncResult> RunSyncAsync(IntegrationConnection connection, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(connection.Provider.Code);
        if (provider is null)
        {
            return new IntegrationSyncResult
            {
                Success = false,
                Message = $"No provider implementation is registered for {connection.Provider.Code}.",
                RecordsProcessed = 0,
            };
        }

        return await provider.RunSyncAsync(connection, cancellationToken);
    }

    public Task<LookupValue?> GetStatusByCodeAsync(string statusCode, CancellationToken cancellationToken = default)
    {
        var normalized = statusCode.Trim().ToUpperInvariant();
        return _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "INTEGRATION_SYNC_STATUS" && x.Code == normalized)
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    public Task<LookupValue?> GetTriggerTypeByCodeAsync(string triggerTypeCode, CancellationToken cancellationToken = default)
    {
        var normalized = triggerTypeCode.Trim().ToUpperInvariant();
        return _dbContext.LookupValues
            .Where(x => x.LookupCategory.Code == "INTEGRATION_TRIGGER_TYPE" && x.Code == normalized)
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    private IIntegrationProvider? GetProvider(string? providerCode)
    {
        var normalized = providerCode?.Trim().ToUpperInvariant();
        return _providers.FirstOrDefault(x => x.ProviderCode.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }
}

public class BasicReachabilityIntegrationProvider : IIntegrationProvider
{
    private readonly HttpClient _httpClient;

    public BasicReachabilityIntegrationProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ProviderCode => "API_MANAGEMENT";

    public async Task<IntegrationActionResultDto> TestConnectionAsync(IntegrationConnection connection, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connection.EndpointUrl))
        {
            return new IntegrationActionResultDto
            {
                Success = false,
                Message = "Endpoint URL is required to test this integration."
            };
        }

        if (!Uri.TryCreate(connection.EndpointUrl, UriKind.Absolute, out var uri))
        {
            return new IntegrationActionResultDto
            {
                Success = false,
                Message = "Endpoint URL is not a valid absolute URI."
            };
        }

        using var request = new HttpRequestMessage(HttpMethod.Head, uri);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        return new IntegrationActionResultDto
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode
                ? "Connection test succeeded."
                : $"Connection test failed with HTTP {(int)response.StatusCode}."
        };
    }

    public async Task<IntegrationSyncResult> RunSyncAsync(IntegrationConnection connection, CancellationToken cancellationToken = default)
    {
        var testResult = await TestConnectionAsync(connection, cancellationToken);
        if (!testResult.Success)
        {
            return new IntegrationSyncResult
            {
                Success = false,
                Message = testResult.Message,
                RecordsProcessed = 0,
            };
        }

        return new IntegrationSyncResult
        {
            Success = true,
            Message = "Sync completed successfully.",
            RecordsProcessed = 1,
        };
    }
}

public class WebhookIntegrationProvider : IIntegrationProvider
{
    public string ProviderCode => "WEBHOOK";

    public Task<IntegrationActionResultDto> TestConnectionAsync(IntegrationConnection connection, CancellationToken cancellationToken = default)
    {
        var valid = Uri.TryCreate(connection.EndpointUrl, UriKind.Absolute, out _);
        return Task.FromResult(new IntegrationActionResultDto
        {
            Success = valid,
            Message = valid ? "Webhook endpoint format is valid." : "Webhook endpoint must be an absolute URL."
        });
    }

    public Task<IntegrationSyncResult> RunSyncAsync(IntegrationConnection connection, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new IntegrationSyncResult
        {
            Success = true,
            Message = "Webhook dispatch simulation completed.",
            RecordsProcessed = 1,
        });
    }
}
