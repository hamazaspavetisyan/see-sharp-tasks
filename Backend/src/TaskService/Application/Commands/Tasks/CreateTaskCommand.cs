using MediatR;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Application.Commands.Tasks;

public record CreateTaskCommand(
    string Name,
    string? Description,
    Guid? CategoryId,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    List<string> Tags,
    Guid UserId) : IRequest<TaskItemDto>;

public class CreateTaskCommandHandler(TasksDbContext db)
    : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Tags = request.Tags,
            UserId = request.UserId
        };
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }
}
