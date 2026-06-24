using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Commands.Categories;

public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest;

public class DeleteCategoryCommandHandler(TasksDbContext db)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(
            c => c.Id == request.Id && c.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Category not found.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }
}
