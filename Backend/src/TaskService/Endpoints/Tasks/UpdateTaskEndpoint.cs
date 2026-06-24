using FastEndpoints;
using MediatR;
using TaskService.Application.Commands.Tasks;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;

namespace TaskService.Endpoints.Tasks;

public class UpdateTaskRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class UpdateTaskEndpoint : Endpoint<UpdateTaskRequest, TaskItemDto>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Put("/api/tasks/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
        try
        {
            var result = await Mediator.Send(
                new UpdateTaskCommand(req.Id, req.Name, req.Description, req.CategoryId,
                    req.Status, req.Priority, req.DueDate, req.Tags, userId), ct);
            await Send.OkAsync(result, ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
    }
}
