using AuthService.Application.Commands;
using AuthService.Application.DTOs;
using FastEndpoints;
using MediatR;

namespace AuthService.Endpoints;

public class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        try
        {
            var result = await Mediator.Send(new LoginCommand(req.Email, req.Password), ct);
            await Send.OkAsync(result, ct);
        }
        catch (UnauthorizedAccessException)
        {
            AddError("Invalid email or password.");
            await Send.ErrorsAsync(401, ct);
        }
    }
}
