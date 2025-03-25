using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Exceptions;

public class TokenException(string message) : BaseException(message, statusCode:498) { }