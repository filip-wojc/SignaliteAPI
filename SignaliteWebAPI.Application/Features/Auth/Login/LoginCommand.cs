using MediatR;
using SignaliteWebAPI.Domain.DTOs.Auth;

namespace SignaliteWebAPI.Application.Features.Auth.Login;

public class LoginCommand : IRequest<LoginResponseDTO>
{
    public LoginDTO LoginDto { get; set; }
}