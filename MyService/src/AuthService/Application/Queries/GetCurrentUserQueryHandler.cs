using AuthService.Application.DTOs;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Queries;

public class GetCurrentUserQueryHandler(AuthDbContext db) : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        return new UserDto(user.Id, user.Email, user.CreatedAt);
    }
}
