using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Exceptions;

public class CloudinaryException(string message) : BaseException(message,512)
{
    
}