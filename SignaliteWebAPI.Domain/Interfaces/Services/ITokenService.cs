using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.Interfaces.Services;

public interface ITokenService
{
    string CreateToken(User user);
}