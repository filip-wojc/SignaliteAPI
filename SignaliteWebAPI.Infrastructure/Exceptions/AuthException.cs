using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Exceptions;

public class AuthException(string message) : BaseException(message, 401)
{
}