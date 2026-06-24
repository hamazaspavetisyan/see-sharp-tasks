using FastEndpoints;
using FluentValidation;
using MediatR;
using TaskService.Application.Commands.Tasks;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;

namespace TaskService.Endpoints.Tasks;

public class CreateTaskRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class CreateTaskRequestValidator : Validator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate is not null);
    }
}

public class CreateTaskEndpoint : Endpoint<CreateTaskRequest, TaskItemDto>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Post("/api/tasks");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
        var result = await Mediator.Send(
            new CreateTaskCommand(req.Name, req.Description, req.CategoryId,
                req.Status, req.Priority, req.DueDate, req.Tags, userId), ct);
        await Send.CreatedAtAsync<GetTaskByIdEndpoint>(
            new { Id = result.Id }, result, generateAbsoluteUrl: false, cancellation: ct);
    }
}
