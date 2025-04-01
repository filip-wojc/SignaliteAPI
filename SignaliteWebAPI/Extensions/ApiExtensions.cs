using Microsoft.AspNetCore.Mvc;
using Serilog;
using SignaliteWebAPI.Middlewares;

namespace SignaliteWebAPI.Extensions;

public static class ApiExtensions
{
    public static void AddApiServices(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddSingleton(Log.Logger);
    }
}