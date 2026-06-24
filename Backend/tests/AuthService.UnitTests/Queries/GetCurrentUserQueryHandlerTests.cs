using AuthService.Application.Queries;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.UnitTests.Queries;

public class GetCurrentUserQueryHandlerTests : IDisposable
{
    private readonly AuthDbContext _db;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AuthDbContext(options);
        _handler = new GetCurrentUserQueryHandler(_db);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        _db.Users.Add(new User
        {
            Id = userId,
            Email = "found@example.com",
            PasswordHash = "hash"
        });
        await _db.SaveChangesAsync();

        var result = await _handler.Handle(new GetCurrentUserQuery(userId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("found@example.com");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        var act = async () => await _handler.Handle(
            new GetCurrentUserQuery(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    public void Dispose() => _db.Dispose();
}
