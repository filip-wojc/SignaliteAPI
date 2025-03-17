using Serilog;
using Serilog.Events;

namespace SignaliteWebAPI.Extensions;

public static class LoggerExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Avoid excessive logs from ASP.NET Core internals
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information) // Log EF Core SQL queries (Redundant)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Information, // change this to filter out the logs
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", 
                theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code// Enables color output
            )
            .WriteTo.File("logs/critical.log", restrictedToMinimumLevel: LogEventLevel.Fatal, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Add Serilog as the default logger
        builder.Host.UseSerilog();
    }

    public static void ConfigureSerilogHttpLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "\n======================== API REQUEST ========================" +
                                      "\n{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms" +
                                      "\n=============================================================\n";

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                {
                    return LogEventLevel.Error; // Log exceptions as Errors
                }

                var statusCode = httpContext.Response.StatusCode;
        
                return statusCode switch
                {
                    >= 500 => LogEventLevel.Error,    // Server errors (500+) as Error
                    >= 400 => LogEventLevel.Warning,  // Client errors (400-499) as Warning
                    _ => LogEventLevel.Information    // Everything else as Information
                };
            };
        });
    }
}