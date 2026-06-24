using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
    public string GenerateToken(User user)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "TasksApp.AuthService";
        var audience = configuration["Jwt:Audience"] ?? "TasksApp";
        var expiryHours = int.TryParse(configuration["Jwt:ExpiryHours"], out var h) ? h : 1;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
