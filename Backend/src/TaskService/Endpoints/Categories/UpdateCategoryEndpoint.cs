using FastEndpoints;
using FluentValidation;
using MediatR;
using TaskService.Application.Commands.Categories;
using TaskService.Application.DTOs;

namespace TaskService.Endpoints.Categories;

public class UpdateCategoryRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

public class UpdateCategoryRequestValidator : Validator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class UpdateCategoryEndpoint : Endpoint<UpdateCategoryRequest, CategoryDto>
{
    private IMediator Mediator => Resolve<IMediator>();

    public override void Configure()
    {
        Put("/api/categories/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateCategoryRequest req, CancellationToken ct)
    {
        var userId = EndpointHelper.GetUserId(HttpContext.Request);
        try
        {
            var result = await Mediator.Send(new UpdateCategoryCommand(req.Id, req.Name, userId), ct);
            await Send.OkAsync(result, ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(409, ct);
        }
    }
}
