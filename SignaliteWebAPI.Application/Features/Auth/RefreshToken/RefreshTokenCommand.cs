using MediatR;
using SignaliteWebAPI.Domain.DTOs.Auth;

namespace SignaliteWebAPI.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<TokenResponseDTO>
{
    public string RefreshToken { get; set; } // idk if dto is needed for that 
}