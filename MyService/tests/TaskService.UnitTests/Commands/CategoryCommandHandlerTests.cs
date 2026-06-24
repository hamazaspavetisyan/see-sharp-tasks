using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Commands.Categories;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.UnitTests.Commands;

public class CategoryCommandHandlerTests
{
    private static TasksDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateCategory_ValidRequest_ReturnsDto()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var handler = new CreateCategoryCommandHandler(db);

        var result = await handler.Handle(new CreateCategoryCommand("Work", userId), CancellationToken.None);

        result.Name.Should().Be("Work");
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateCategory_DuplicateName_Throws()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        db.Categories.Add(new Category { Name = "Work", UserId = userId });
        await db.SaveChangesAsync();

        var handler = new CreateCategoryCommandHandler(db);
        var act = () => handler.Handle(new CreateCategoryCommand("Work", userId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteCategory_ExistingCategory_Removes()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var cat = new Category { Name = "Work", UserId = userId };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var handler = new DeleteCategoryCommandHandler(db);
        await handler.Handle(new DeleteCategoryCommand(cat.Id, userId), CancellationToken.None);

        (await db.Categories.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteCategory_NotOwned_Throws()
    {
        await using var db = CreateDb();
        var cat = new Category { Name = "Work", UserId = Guid.NewGuid() };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var handler = new DeleteCategoryCommandHandler(db);
        var act = () => handler.Handle(new DeleteCategoryCommand(cat.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
