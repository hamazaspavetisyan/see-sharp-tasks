using FastEndpoints;
using MediatR;
using TaskService.Application.DTOs;
using TaskService.Application.Queries;

namespace TaskService.Endpoints.Categories;

public class GetCategoriesEndpoint : EndpointWithoutRequest<List<CategoryDto>>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Get("/api/categories");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(User);
        var result = await Mediator.Send(new GetCategoriesQuery(userId), ct);
        await Send.OkAsync(result, ct);
    }
}
