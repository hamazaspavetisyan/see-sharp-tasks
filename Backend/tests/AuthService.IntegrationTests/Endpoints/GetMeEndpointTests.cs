using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Application.DTOs;
using FluentAssertions;

namespace AuthService.IntegrationTests.Endpoints;

public class GetMeEndpointTests(AuthServiceFactory factory) : IClassFixture<AuthServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> RegisterAndLoginAsync(string email, string password)
    {
        await _client.PostAsJsonAsync("/api/auth/register", new { Email = email, Password = password });
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task GetMe_ValidToken_Returns200WithUserInfo()
    {
        var token = await RegisterAndLoginAsync("me@example.com", "ValidPass1!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("me@example.com");
    }

    [Fact]
    public async Task GetMe_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_InvalidToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
