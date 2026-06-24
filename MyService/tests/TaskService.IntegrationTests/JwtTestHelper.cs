using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskService.IntegrationTests;

public static class JwtTestHelper
{
    private const string Secret = "test-integration-secret-key-at-least-32-chars!!";
    private const string Issuer = "TasksApp.AuthService";
    private const string Audience = "TasksApp";

    public static string GenerateToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, $"{userId}@test.com")
        };
        var token = new JwtSecurityToken(Issuer, Audience, claims,
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
