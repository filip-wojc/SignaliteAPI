using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Infrastructure.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string GenerateAccessToken(User user)
    {
        if (user.Username == null)
            throw new TokenException("No username present for user while creating token");

        var tokenKey = config["TokenKey"] ?? throw new ConfigException("Cannot access token key from appsettings.json");
      
        if (tokenKey.Length < 64) 
            throw new ConfigException("Token key needs to be at least 64 characters long");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

      
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
        };

        // For roles if we decide to use identity
        // var roles = await userManager.GetRolesAsync(user);
        // claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role))); // add role info to the token

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenKey = config["TokenKey"] ?? throw new ConfigException("Cannot access token key from appsettings.json");
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true // We're validating an expired token here
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            throw new TokenException("Invalid token");

        return principal;
    }
    
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var tokenKey = config["TokenKey"] ?? throw new ConfigException("Cannot access token key from appsettings.json");
      
        if (tokenKey.Length < 64) 
            throw new ConfigException("Token key needs to be at least 64 characters long");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Shorter lifetime for access tokens
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

