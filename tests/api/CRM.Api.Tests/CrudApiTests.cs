using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace CRM.Api.Tests;

public class CrudApiTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public CrudApiTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Crud")]
    public async Task Users_WithAdminToken_ReturnsSuccess()
    {
        var token = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Crud")]
    [Trait("Category", "Sequence")]
    public async Task NumberSequence_Create_WithInvalidCode_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/number-sequences")
        {
            Content = JsonContent.Create(new
            {
                entityName = "Test Entity",
                sequenceCode = "invalid code",
                prefix = "TST",
                suffix = "",
                separator = "-",
                minimumDigits = 4,
                currentNumber = 0,
                resetFrequencyId = (Guid?)null,
                resetDayOfMonth = (int?)null,
                resetMonth = (int?)null,
                isActive = true,
            }),
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", "Crud")]
    [Trait("Category", "Lookup")]
    public async Task LookupValue_Details_ForUnknownId_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lookup-values/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@crm.local",
            password = "Admin@12345",
        });

        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponsePayload>();
        auth.Should().NotBeNull();
        return auth!.AccessToken;
    }

    private sealed class AuthResponsePayload
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
