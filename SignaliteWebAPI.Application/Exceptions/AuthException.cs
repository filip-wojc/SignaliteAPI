using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Application.Exceptions;

public class AuthException(string message) : BaseException(message, 401)
{
}