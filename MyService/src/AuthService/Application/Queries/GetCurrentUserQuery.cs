using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Queries;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
