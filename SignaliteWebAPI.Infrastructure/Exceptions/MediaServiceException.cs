using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Exceptions;

public class MediaServiceException(string message) : BaseException(message, 500)
{
    
}