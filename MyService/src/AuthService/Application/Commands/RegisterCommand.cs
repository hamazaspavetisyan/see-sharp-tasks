using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands;

public record RegisterCommand(string Email, string Password) : IRequest<RegisterResponse>;
