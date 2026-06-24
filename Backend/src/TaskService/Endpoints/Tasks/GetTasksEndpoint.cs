using FastEndpoints;
using FluentValidation;
using MediatR;
using TaskService.Application.DTOs;
using TaskService.Application.Queries;
using TaskService.Domain.Entities;

namespace TaskService.Endpoints.Tasks;

public class GetTasksRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
}

public class GetTasksRequestValidator : Validator<GetTasksRequest>
{
    public GetTasksRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class GetTasksEndpoint : Endpoint<GetTasksRequest, PagedResult<TaskItemDto>>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Get("/api/tasks");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTasksRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
        var result = await Mediator.Send(
            new GetTasksQuery(userId, req.Page, req.PageSize, req.Status, req.Priority, req.CategoryId, req.Search), ct);
        await Send.OkAsync(result, ct);
    }
}
