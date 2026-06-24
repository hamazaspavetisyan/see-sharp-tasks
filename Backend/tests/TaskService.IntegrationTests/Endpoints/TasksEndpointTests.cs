using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;

namespace TaskService.IntegrationTests.Endpoints;

public class TasksEndpointTests(TaskServiceFactory factory) : IClassFixture<TaskServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Guid _userId = Guid.NewGuid();

    private void Authenticate() =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(_userId));

    [Fact]
    public async Task GetTasks_Authenticated_Returns200()
    {
        Authenticate();
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTasks_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_ValidRequest_Returns201()
    {
        Authenticate();
        // Priority: High=2, Status: Todo=0
        var payload = new { Name = "Integration Task", Priority = 2, Status = 0, Tags = new[] { "test" } };
        var response = await _client.PostAsJsonAsync("/api/tasks", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAndGetTask_ReturnsCreatedTask()
    {
        Authenticate();
        // Priority: Low=0, Status: Todo=0
        var payload = new { Name = "Find Me", Priority = 0, Status = 0, Tags = Array.Empty<string>() };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        created.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/tasks/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        fetched!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetTaskById_OtherUsersTask_Returns404()
    {
        Authenticate();
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_Returns204()
    {
        Authenticate();
        var payload = new { Name = "Delete Me", Priority = 0, Status = 0, Tags = Array.Empty<string>() };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", payload);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
