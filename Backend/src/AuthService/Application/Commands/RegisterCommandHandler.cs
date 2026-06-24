using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class RegisterCommandHandler(AuthDbContext db) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == emailLower, cancellationToken))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Email = emailLower,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);// xxxx todo, why not SaveChanges but async ?

        return new RegisterResponse(user.Id, user.Email);
    }
}
