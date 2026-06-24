using TaskService.Domain.Entities;

namespace TaskService.Application.DTOs;

public record TaskItemDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    List<string> Tags,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
