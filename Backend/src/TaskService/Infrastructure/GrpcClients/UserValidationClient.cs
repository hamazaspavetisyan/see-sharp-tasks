using TaskService.Application.Services;
using TaskService.Protos;

namespace TaskService.Infrastructure.GrpcClients;

public class UserValidationClient(UserValidation.UserValidationClient grpcClient)
    : IUserValidationService
{
    public async Task<bool> ValidateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await grpcClient.ValidateUserAsync(
            new ValidateUserRequest { UserId = userId.ToString() }, cancellationToken: ct);// xxxx todo
        return response.IsValid;
    }
}
