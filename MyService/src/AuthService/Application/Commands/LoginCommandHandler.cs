using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class LoginCommandHandler(AuthDbContext db, IJwtService jwtService, IConfiguration config)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == emailLower, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = jwtService.GenerateToken(user);
        var expiryHours = int.TryParse(config["Jwt:ExpiryHours"], out var h) ? h : 1;

        return new LoginResponse(token, DateTime.UtcNow.AddHours(expiryHours));
    }
}
