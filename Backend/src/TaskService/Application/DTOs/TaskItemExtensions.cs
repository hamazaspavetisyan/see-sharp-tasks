using TaskService.Domain.Entities;

namespace TaskService.Application.DTOs;

public static class TaskItemExtensions
{
    public static TaskItemDto ToDto(this TaskItem t) =>
        new(t.Id, t.Name, t.Description, t.CategoryId, t.Category?.Name,
            t.Status, t.Priority, t.DueDate, t.Tags, t.UserId, t.CreatedAt, t.UpdatedAt);
}
