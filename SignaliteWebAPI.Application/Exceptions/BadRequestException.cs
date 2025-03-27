using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Application.Exceptions;

public class BadRequestException(string message) : BaseException(message, statusCode: 400)
{
    
}