using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Commands.Tasks;

public record UpdateTaskCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? CategoryId,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    List<string> Tags,
    Guid UserId) : IRequest<TaskItemDto>;

public class UpdateTaskCommandHandler(TasksDbContext db)
    : IRequestHandler<UpdateTaskCommand, TaskItemDto>
{
    public async Task<TaskItemDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");

        task.Name = request.Name;
        task.Description = request.Description;
        task.CategoryId = request.CategoryId;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.Tags = request.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }
}
