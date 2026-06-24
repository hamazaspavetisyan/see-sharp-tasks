using Microsoft.AspNetCore.Http;

namespace AuthService.Endpoints;

public static class EndpointHelper
{
    public static Guid GetUserId(HttpRequest request)
    {
        var value = request.Headers["X-User-Id"].FirstOrDefault();
        if (value is null || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Missing or invalid X-User-Id header.");
        return userId;
    }
}
