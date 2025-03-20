using System.Security.Claims;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}