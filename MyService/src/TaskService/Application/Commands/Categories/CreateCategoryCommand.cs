using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Commands.Categories;

public record CreateCategoryCommand(string Name, Guid UserId) : IRequest<CategoryDto>;

public class CreateCategoryCommandHandler(TasksDbContext db)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var exists = await db.Categories.AnyAsync(
            c => c.UserId == request.UserId && c.Name == request.Name, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Category '{request.Name}' already exists.");

        var category = new Category
        {
            Name = request.Name,
            UserId = request.UserId
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.UserId, category.CreatedAt);
    }
}
