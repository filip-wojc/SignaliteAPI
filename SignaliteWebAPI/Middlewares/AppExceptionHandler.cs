using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using SignaliteWebAPI.Exceptions;

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
            // TODO Add more cases
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