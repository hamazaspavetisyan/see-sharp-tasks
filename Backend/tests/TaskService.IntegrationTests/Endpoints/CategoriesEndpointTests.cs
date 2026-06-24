using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskService.Application.DTOs;

namespace TaskService.IntegrationTests.Endpoints;

public class CategoriesEndpointTests(TaskServiceFactory factory) : IClassFixture<TaskServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Guid _userId = Guid.NewGuid();

    private void Authenticate() => JwtTestHelper.SetUserId(_client, _userId);

    [Fact]
    public async Task GetCategories_Authenticated_Returns200()
    {
        Authenticate();
        var response = await _client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategories_NoUserId_Returns401()
    {
        JwtTestHelper.ClearUserId(_client);
        var response = await _client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_ValidRequest_Returns200()
    {
        Authenticate();
        var response = await _client.PostAsJsonAsync("/api/categories", new { Name = "Work" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCategory_EmptyName_Returns400()
    {
        Authenticate();
        var response = await _client.PostAsJsonAsync("/api/categories", new { Name = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_NameTooLong_Returns400()
    {
        Authenticate();
        var response = await _client.PostAsJsonAsync("/api/categories", new { Name = new string('a', 101) });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_Duplicate_Returns409()
    {
        Authenticate();
        await _client.PostAsJsonAsync("/api/categories", new { Name = "Unique" });
        var response = await _client.PostAsJsonAsync("/api/categories", new { Name = "Unique" });
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateCategory_EmptyName_Returns400()
    {
        Authenticate();
        var created = await (await _client.PostAsJsonAsync("/api/categories", new { Name = "ToUpdate" }))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var response = await _client.PutAsJsonAsync($"/api/categories/{created!.Id}", new { Name = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_NameTooLong_Returns400()
    {
        Authenticate();
        var created = await (await _client.PostAsJsonAsync("/api/categories", new { Name = "ToUpdateLong" }))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var response = await _client.PutAsJsonAsync($"/api/categories/{created!.Id}", new { Name = new string('a', 101) });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_Returns204()
    {
        Authenticate();
        var created = await (await _client.PostAsJsonAsync("/api/categories", new { Name = "ToDelete" }))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/categories/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCategory_NonExistent_Returns404()
    {
        Authenticate();
        var response = await _client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
