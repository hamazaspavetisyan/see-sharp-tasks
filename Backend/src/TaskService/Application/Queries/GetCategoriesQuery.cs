using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.DTOs;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Queries;

public record GetCategoriesQuery(Guid UserId) : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler(TasksDbContext db)
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await db.Categories
            .Where(c => c.UserId == request.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.UserId, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
