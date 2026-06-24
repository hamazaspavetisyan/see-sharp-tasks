using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.DTOs;
using AuthService.Application.Queries;
using FastEndpoints;
using MediatR;

namespace AuthService.Endpoints;

public class GetMeEndpoint : EndpointWithoutRequest<UserDto>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Get("/api/auth/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            ThrowError("Unauthorized.", 401);
            return;
        }

        try
        {
            var result = await Mediator.Send(new GetCurrentUserQuery(userId), ct);
            await Send.OkAsync(result, ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
    }
}
