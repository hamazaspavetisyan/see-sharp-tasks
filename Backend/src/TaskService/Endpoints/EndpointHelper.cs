using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskService.Endpoints;

public static class EndpointHelper
{
    public static Guid GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim is null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");
        return userId;
    }
}
