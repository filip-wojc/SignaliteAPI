using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Exceptions;

public class ConfigException(string message) : BaseException(message, statusCode:500)
{
    
}