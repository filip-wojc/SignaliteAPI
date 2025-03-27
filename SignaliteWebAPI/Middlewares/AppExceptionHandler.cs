using System.Security.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Exceptions;
using SignaliteWebAPI.Exceptions;
using SignaliteWebAPI.Infrastructure.Exceptions;

namespace SignaliteWebAPI.Middlewares;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string message;
        List<string> errorList = [];

        if (exception is BaseException ex)
        {
            statusCode = ex.StatusCode;
            message = ex.Message;
            errorList = ex.Errors;
        }
        else
        {
            statusCode = 500;
            message = exception.Message;
        }
        
        
        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errorList
        };
        
        httpContext.Response.ContentType = "application/json";
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken: cancellationToken);
        
        return true;
    }
}