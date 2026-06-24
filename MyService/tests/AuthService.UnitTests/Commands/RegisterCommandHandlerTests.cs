using AuthService.Application.Commands;
using AuthService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.UnitTests.Commands;

public class RegisterCommandHandlerTests : IDisposable
{
    private readonly AuthDbContext _db;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AuthDbContext(options);
        _handler = new RegisterCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUser()
    {
        var command = new RegisterCommand("test@example.com", "Password123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.Id.Should().NotBeEmpty();
        _db.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowerCase()
    {
        var command = new RegisterCommand("Test@Example.COM", "Password123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var command = new RegisterCommand("dup@example.com", "Password123!");
        await _handler.Handle(command, CancellationToken.None);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task Handle_HashesPassword()
    {
        var command = new RegisterCommand("hash@example.com", "MyPassword");

        await _handler.Handle(command, CancellationToken.None);

        var user = await _db.Users.FirstAsync();
        user.PasswordHash.Should().NotBe("MyPassword");
        BCrypt.Net.BCrypt.Verify("MyPassword", user.PasswordHash).Should().BeTrue();
    }

    public void Dispose() => _db.Dispose();
}
