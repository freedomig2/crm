using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;

namespace CRM.Api.Tests;

public class AuthApiTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Auth")]
    public async Task Login_WithAdminCredentials_ReturnsAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@crm.local",
            password = "Admin@12345",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponsePayload>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Auth")]
    [Trait("Category", "Permission")]
    public async Task Users_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Auth")]
    [Trait("Category", "Permission")]
    public async Task Users_WithAuthenticatedButUnprivilegedUser_ReturnsForbidden()
    {
        var faker = new Faker();
        var email = $"qa.{Guid.NewGuid():N}@crm.local";

        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "QaUser@12345",
            firstName = faker.Name.FirstName(),
            lastName = faker.Name.LastName(),
        });

        register.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "QaUser@12345",
        });

        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponsePayload>();
        auth.Should().NotBeNull();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed class AuthResponsePayload
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
