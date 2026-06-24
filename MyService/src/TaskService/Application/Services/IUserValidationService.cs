namespace TaskService.Application.Services;

public interface IUserValidationService
{
    Task<bool> ValidateUserAsync(Guid userId, CancellationToken ct = default);
}
