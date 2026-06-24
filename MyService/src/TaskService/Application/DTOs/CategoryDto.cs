namespace TaskService.Application.DTOs;

public record CategoryDto(Guid Id, string Name, Guid UserId, DateTime CreatedAt);
