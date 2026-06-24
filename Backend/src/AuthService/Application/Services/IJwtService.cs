using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
