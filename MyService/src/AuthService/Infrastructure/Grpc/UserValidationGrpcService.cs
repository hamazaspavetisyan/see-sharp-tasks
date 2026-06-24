using AuthService.Infrastructure.Persistence;
using AuthService.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Grpc;

public class UserValidationGrpcService(AuthDbContext db) : UserValidation.UserValidationBase
{
    public override async Task<ValidateUserResponse> ValidateUser(
        ValidateUserRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return new ValidateUserResponse { IsValid = false, Email = string.Empty };

        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Id == userId,
            context.CancellationToken);

        return user is null
            ? new ValidateUserResponse { IsValid = false, Email = string.Empty }
            : new ValidateUserResponse { IsValid = true, Email = user.Email };
    }
}
