using MediatR;
using SignaliteWebAPI.Domain.DTOs.Auth;

namespace SignaliteWebAPI.Application.Features.Auth.Login;

public class LoginCommand : IRequest<TokenResponseDTO>
{
    public LoginDTO LoginDto { get; set; }
}