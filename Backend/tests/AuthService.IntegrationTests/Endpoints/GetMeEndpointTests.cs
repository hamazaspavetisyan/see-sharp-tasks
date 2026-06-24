using System.Net;
using System.Net.Http.Json;
using AuthService.Application.DTOs;
using FluentAssertions;

namespace AuthService.IntegrationTests.Endpoints;

public class GetMeEndpointTests(AuthServiceFactory factory) : IClassFixture<AuthServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> RegisterAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { Email = email, Password = password });
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return body!.Id;
    }

    [Fact]
    public async Task GetMe_ValidUserId_Returns200WithUserInfo()
    {
        var userId = await RegisterAsync("me@example.com", "ValidPass1!");
        _client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("me@example.com");
    }

    [Fact]
    public async Task GetMe_NoUserId_Returns401()
    {
        _client.DefaultRequestHeaders.Remove("X-User-Id");

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_UnknownUserId_Returns404()
    {
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
