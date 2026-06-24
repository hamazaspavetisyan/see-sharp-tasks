using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public interface IJwtService //xxxx todo
{
    string GenerateToken(User user);
}
