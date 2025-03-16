using SignaliteWebAPI.Domain.Exceptions;

namespace SignaliteWebAPI.Application.Exceptions;

public class ValidatorException(string message) : BaseException(message, statusCode:400) { }