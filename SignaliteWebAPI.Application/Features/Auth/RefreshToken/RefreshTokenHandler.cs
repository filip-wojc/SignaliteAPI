using MediatR;
using SignaliteWebAPI.Domain.DTOs.Auth;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces;

namespace SignaliteWebAPI.Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
    IUserRepository userRepository, 
    ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, TokenResponseDTO>
{
    public async Task<TokenResponseDTO> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the user with this refresh token
        var user = await userRepository.GetUserByRefreshToken(request.RefreshToken);
        
        if (user == null)
            throw new TokenException("Invalid refresh token");
            
        if (user.RefreshTokenExpiryDate <= DateTime.UtcNow)
            throw new TokenException("Refresh token expired"); 
            
        // Generate new tokens
        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken(); // renew refresh token
        
        // Set refresh token expiry to 30 days from now
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        // Update the refresh token in database
        await userRepository.UpdateRefreshToken(user.Id, newRefreshToken, refreshTokenExpiry);
        
        return new TokenResponseDTO
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddDays(7)
        };
    }
}
