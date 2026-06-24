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
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
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
