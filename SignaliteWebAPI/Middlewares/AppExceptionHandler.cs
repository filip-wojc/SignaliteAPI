using System.Security.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using SignaliteWebAPI.Application.Exceptions;
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
        
        switch (exception)
        {
            case ValidatorException ex:
            {
                statusCode = ex.StatusCode;
                message = ex.Message;
                errorList = ex.Errors;
                break;
            }
            case AuthException ex:
            {
                statusCode = ex.StatusCode;
                message = ex.Message;
                errorList = ex.Errors;
                break;
            }
            case TokenException ex:
            {
                statusCode = ex.StatusCode;
                message = ex.Message;
                errorList = ex.Errors;
                break;
            }
            case NotFoundException ex:
            {
                statusCode = ex.StatusCode;
                message = ex.Message;
                errorList = ex.Errors;
                break;
            }
            case ForbidException ex:
            {
                statusCode = ex.StatusCode;
                message = ex.Message;
                errorList = ex.Errors;
                break;
            }
            default:
            {           
                statusCode = 500;
                message = exception.Message;
                break;
            } 
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
        await httpContext.Response.WriteAsync(jsonResponse);
        
        return true;
    }
}