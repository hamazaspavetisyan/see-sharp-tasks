using FastEndpoints;
using MediatR;
using TaskService.Application.DTOs;
using TaskService.Application.Queries;

namespace TaskService.Endpoints.Tasks;

public class GetTaskByIdRequest
{
    public Guid Id { get; set; }
}

public class GetTaskByIdEndpoint : Endpoint<GetTaskByIdRequest, TaskItemDto>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Get("/api/tasks/{Id}");
    }

    public override async Task HandleAsync(GetTaskByIdRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(User);
        try
        {
            var result = await Mediator.Send(new GetTaskByIdQuery(req.Id, userId), ct);
            await Send.OkAsync(result, ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
    }
}
