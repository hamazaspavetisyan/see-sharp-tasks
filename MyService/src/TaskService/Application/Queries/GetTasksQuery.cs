using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Queries;

public record GetTasksQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    Guid? CategoryId = null,
    string? Search = null) : IRequest<PagedResult<TaskItemDto>>;

public class GetTasksQueryHandler(TasksDbContext db)
    : IRequestHandler<GetTasksQuery, PagedResult<TaskItemDto>>
{
    public async Task<PagedResult<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = db.Tasks.Include(t => t.Category).Where(t => t.UserId == request.UserId);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);
        if (request.Priority.HasValue)
            query = query.Where(t => t.Priority == request.Priority.Value);
        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.Contains(request.Search) ||
                                     (t.Description != null && t.Description.Contains(request.Search)));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => t.ToDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItemDto>(items, total, request.Page, request.PageSize);
    }
}
