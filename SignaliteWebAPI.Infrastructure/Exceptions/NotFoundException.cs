using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Exceptions;

public class NotFoundException(string message) : BaseException(message, statusCode: 404)
{
    
}