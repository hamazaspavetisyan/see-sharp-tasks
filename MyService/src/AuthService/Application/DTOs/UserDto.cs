namespace AuthService.Application.DTOs;

public record UserDto(Guid Id, string Email, DateTime CreatedAt);
