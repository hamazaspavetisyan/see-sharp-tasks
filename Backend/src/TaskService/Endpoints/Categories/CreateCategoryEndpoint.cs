using FastEndpoints;
using MediatR;
using TaskService.Application.Commands.Categories;
using TaskService.Application.DTOs;

namespace TaskService.Endpoints.Categories;

public class CreateCategoryRequest
{
    public string Name { get; set; } = default!;
}

public class CreateCategoryEndpoint : Endpoint<CreateCategoryRequest, CategoryDto>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Post("/api/categories");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
        try
        {
            var result = await Mediator.Send(new CreateCategoryCommand(req.Name, userId), ct);
            await Send.OkAsync(result, ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(409, ct);
        }
    }
}
