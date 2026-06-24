using FastEndpoints;
using MediatR;
using TaskService.Application.Commands.Tasks;

namespace TaskService.Endpoints.Tasks;

public class DeleteTaskRequest
{
    public Guid Id { get; set; }
}

public class DeleteTaskEndpoint : Endpoint<DeleteTaskRequest>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Delete("/api/tasks/{Id}");
    }

    public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(User);
        try
        {
            await Mediator.Send(new DeleteTaskCommand(req.Id, userId), ct);
            await Send.NoContentAsync(ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
    }
}
