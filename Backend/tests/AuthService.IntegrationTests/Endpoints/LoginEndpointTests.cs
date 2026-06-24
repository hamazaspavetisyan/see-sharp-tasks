using System.Net;
using System.Net.Http.Json;
using AuthService.Application.DTOs;
using FluentAssertions;

namespace AuthService.IntegrationTests.Endpoints;

public class LoginEndpointTests(AuthServiceFactory factory)
    : IClassFixture<AuthServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task RegisterAsync(string email, string password)
    {
        await _client.PostAsJsonAsync("/api/auth/register", new { Email = email, Password = password });
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        await RegisterAsync("login@example.com", "ValidPass1!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "login@example.com",
            Password = "ValidPass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await RegisterAsync("wrongpass@example.com", "CorrectPass1!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "wrongpass@example.com",
            Password = "WrongPass!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nobody@example.com",
            Password = "SomePass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "not-an-email",
            Password = "SomePass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_EmptyEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "",
            Password = "SomePass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_EmptyPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "empty@example.com",
            Password = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
