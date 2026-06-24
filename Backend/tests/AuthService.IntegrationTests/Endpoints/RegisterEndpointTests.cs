using System.Net;
using System.Net.Http.Json;
using AuthService.Application.DTOs;
using FluentAssertions;

namespace AuthService.IntegrationTests.Endpoints;

public class RegisterEndpointTests(AuthServiceFactory factory)
    : IClassFixture<AuthServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidRequest_Returns200WithUserId()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "new@example.com",
            Password = "Test123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var payload = new { Email = "dup@example.com", Password = "Test123!" };
        await _client.PostAsJsonAsync("/api/auth/register", payload);

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_MissingBody_Returns400()
    {
        var response = await _client.PostAsync("/api/auth/register",
            new StringContent("", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }
}
