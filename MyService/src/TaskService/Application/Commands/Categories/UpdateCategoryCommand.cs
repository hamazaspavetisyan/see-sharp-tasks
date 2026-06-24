using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.DTOs;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Commands.Categories;

public record UpdateCategoryCommand(Guid Id, string Name, Guid UserId) : IRequest<CategoryDto>;

public class UpdateCategoryCommandHandler(TasksDbContext db)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(
            c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found.");

        var nameConflict = await db.Categories.AnyAsync(
            c => c.UserId == request.UserId && c.Name == request.Name && c.Id != request.Id,
            cancellationToken);
        if (nameConflict)
            throw new InvalidOperationException($"Category '{request.Name}' already exists.");

        category.Name = request.Name;
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.UserId, category.CreatedAt);
    }
}
