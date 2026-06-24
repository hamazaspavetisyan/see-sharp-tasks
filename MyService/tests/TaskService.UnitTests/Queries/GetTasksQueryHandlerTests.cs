using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Queries;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.UnitTests.Queries;

public class GetTasksQueryHandlerTests
{
    private static TasksDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetTasks_ReturnsOnlyUserTasks()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        db.Tasks.AddRange(
            new TaskItem { Name = "My Task", UserId = userId },
            new TaskItem { Name = "Other Task", UserId = Guid.NewGuid() });
        await db.SaveChangesAsync();

        var handler = new GetTasksQueryHandler(db);
        var result = await handler.Handle(new GetTasksQuery(userId), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("My Task");
    }

    [Fact]
    public async Task GetTasks_FilterByStatus_ReturnsMatching()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        db.Tasks.AddRange(
            new TaskItem { Name = "Todo", Status = TaskItemStatus.Todo, UserId = userId },
            new TaskItem { Name = "Done", Status = TaskItemStatus.Done, UserId = userId });
        await db.SaveChangesAsync();

        var handler = new GetTasksQueryHandler(db);
        var result = await handler.Handle(new GetTasksQuery(userId, Status: TaskItemStatus.Done), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Done");
    }

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsDto()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var task = new TaskItem { Name = "My Task", UserId = userId };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new GetTaskByIdQueryHandler(db);
        var result = await handler.Handle(new GetTaskByIdQuery(task.Id, userId), CancellationToken.None);

        result.Id.Should().Be(task.Id);
        result.Name.Should().Be("My Task");
    }

    [Fact]
    public async Task GetTaskById_NotFound_Throws()
    {
        await using var db = CreateDb();
        var handler = new GetTaskByIdQueryHandler(db);

        var act = () => handler.Handle(new GetTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetTasks_SearchByName_ReturnsMatching()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        db.Tasks.AddRange(
            new TaskItem { Name = "Buy milk", UserId = userId },
            new TaskItem { Name = "Do laundry", UserId = userId });
        await db.SaveChangesAsync();

        var handler = new GetTasksQueryHandler(db);
        var result = await handler.Handle(
            new GetTasksQuery(userId, Search: "milk"), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Buy milk");
    }
}
