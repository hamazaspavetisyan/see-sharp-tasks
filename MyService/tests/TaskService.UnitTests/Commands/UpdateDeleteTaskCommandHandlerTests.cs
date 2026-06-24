using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Commands.Tasks;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.UnitTests.Commands;

public class UpdateDeleteTaskCommandHandlerTests
{
    private static TasksDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task UpdateTask_ExistingTask_UpdatesFields()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var task = new TaskItem { Name = "Old", UserId = userId };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new UpdateTaskCommandHandler(db);
        var result = await handler.Handle(
            new UpdateTaskCommand(task.Id, "New Name", "desc", null,
                TaskItemStatus.InProgress, TaskPriority.High, null, [], userId),
            CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task UpdateTask_NotOwned_ThrowsKeyNotFound()
    {
        await using var db = CreateDb();
        var task = new TaskItem { Name = "Task", UserId = Guid.NewGuid() };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new UpdateTaskCommandHandler(db);
        var act = () => handler.Handle(
            new UpdateTaskCommand(task.Id, "New", null, null,
                TaskItemStatus.Todo, TaskPriority.Low, null, [], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_Removes()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var task = new TaskItem { Name = "Task", UserId = userId };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new DeleteTaskCommandHandler(db);
        await handler.Handle(new DeleteTaskCommand(task.Id, userId), CancellationToken.None);

        (await db.Tasks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteTask_NotOwned_ThrowsKeyNotFound()
    {
        await using var db = CreateDb();
        var task = new TaskItem { Name = "Task", UserId = Guid.NewGuid() };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new DeleteTaskCommandHandler(db);
        var act = () => handler.Handle(new DeleteTaskCommand(task.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
