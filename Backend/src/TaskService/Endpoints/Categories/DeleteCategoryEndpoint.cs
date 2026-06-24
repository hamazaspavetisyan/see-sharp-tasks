using FastEndpoints;
using MediatR;
using TaskService.Application.Commands.Categories;

namespace TaskService.Endpoints.Categories;

public class DeleteCategoryRequest
{
    public Guid Id { get; set; }
}

public class DeleteCategoryEndpoint : Endpoint<DeleteCategoryRequest>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Delete("/api/categories/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
        try
        {
            await Mediator.Send(new DeleteCategoryCommand(req.Id, userId), ct);
            await Send.NoContentAsync(ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
    }
}
