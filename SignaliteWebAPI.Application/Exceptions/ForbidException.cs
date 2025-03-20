using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Application.Exceptions;

public class ForbidException(string message) : BaseException(message, statusCode:403)
{
    
}