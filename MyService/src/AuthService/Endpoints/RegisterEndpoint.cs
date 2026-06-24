using AuthService.Application.Commands;
using AuthService.Application.DTOs;
using FastEndpoints;
using MediatR;

namespace AuthService.Endpoints;

public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var result = await Mediator.Send(new RegisterCommand(req.Email, req.Password), ct);
            await Send.OkAsync(result, ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(409, ct);
        }
    }
}
