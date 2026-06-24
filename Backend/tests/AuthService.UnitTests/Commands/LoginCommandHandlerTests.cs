using AuthService.Application.Commands;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthService.UnitTests.Commands;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly AuthDbContext _db;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly IConfiguration _config;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AuthDbContext(options);
        _jwtMock = new Mock<IJwtService>();
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");

        var configData = new Dictionary<string, string?> { ["Jwt:ExpiryHours"] = "1" };
        _config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        _handler = new LoginCommandHandler(_db, _jwtMock.Object, _config);
    }

    private async Task SeedUserAsync(string email, string password)
    {
        _db.Users.Add(new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        await SeedUserAsync("user@example.com", "correct-password");

        var result = await _handler.Handle(
            new LoginCommand("user@example.com", "correct-password"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        await SeedUserAsync("user@example.com", "correct-password");

        var act = async () => await _handler.Handle(
            new LoginCommand("user@example.com", "wrong-password"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        var act = async () => await _handler.Handle(
            new LoginCommand("nobody@example.com", "any-password"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_EmailIsCaseInsensitive()
    {
        await SeedUserAsync("lower@example.com", "pass");

        var result = await _handler.Handle(
            new LoginCommand("LOWER@EXAMPLE.COM", "pass"),
            CancellationToken.None);

        result.Token.Should().Be("fake-jwt-token");
    }

    public void Dispose() => _db.Dispose();
}
