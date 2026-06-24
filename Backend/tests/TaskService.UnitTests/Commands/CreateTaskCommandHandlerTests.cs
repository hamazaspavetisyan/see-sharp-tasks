using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Commands.Tasks;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.UnitTests.Commands;

public class CreateTaskCommandHandlerTests
{
    private static TasksDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        await using var db = CreateDb();
        var handler = new CreateTaskCommandHandler(db);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateTaskCommand("Buy milk", null, null, TaskItemStatus.Todo, TaskPriority.Low, null, [], userId),
            CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Buy milk");
        result.UserId.Should().Be(userId);
        (await db.Tasks.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithTags_PersistsTags()
    {
        await using var db = CreateDb();
        var handler = new CreateTaskCommandHandler(db);
        var tags = new List<string> { "urgent", "work" };

        var result = await handler.Handle(
            new CreateTaskCommand("Task", null, null, TaskItemStatus.Todo, TaskPriority.High, null, tags, Guid.NewGuid()),
            CancellationToken.None);

        result.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public async Task Handle_SetsDefaultStatus_Todo()
    {
        await using var db = CreateDb();
        var handler = new CreateTaskCommandHandler(db);

        var result = await handler.Handle(
            new CreateTaskCommand("Task", null, null, TaskItemStatus.Todo, TaskPriority.Medium, null, [], Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be(TaskItemStatus.Todo);
    }
}
