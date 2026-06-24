using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.DTOs;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Queries;

public record GetTaskByIdQuery(Guid Id, Guid UserId) : IRequest<TaskItemDto>;

public class GetTaskByIdQueryHandler(TasksDbContext db)
    : IRequestHandler<GetTaskByIdQuery, TaskItemDto>
{
    public async Task<TaskItemDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");
        return task.ToDto();
    }
}
