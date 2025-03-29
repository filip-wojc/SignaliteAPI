using SignaliteWebAPI.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using SignaliteWebAPI.Application.Extensions;
using SignaliteWebAPI.Extensions;
using SignaliteWebAPI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.HttpLogging;
using SignaliteWebAPI.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Set up Serilog Configuration
builder.ConfigureSerilog();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});

// Add services to the container.
builder.Services.AddApiServices(); // extension
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddInfrastructureServices(builder.Configuration); // extension function
builder.Services.AddApplicationServices(); // extension function
builder.Services.AddValidatorExtensions();
builder.Services.AddIdentityServices(builder.Configuration); // extension function (configures bearer)


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // http://localhost:5026/scalar/v1
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Signalite APi")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
           
    });
}

app.ConfigureSerilogHttpLogging(); // extension
app.UseHttpLogging(); // Logs request & response headers, body, etc.
app.UseExceptionHandler(_ => { });
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials() // allow credentials to make passing the token to SignalR hubs possible
    .WithOrigins("http://localhost:4200", "https://localhost:4200")); // must be declared before MapControllers() to work
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<NotificationsHub>("hubs/notifications");


bool cleanupPerformed = false;

if (cleanupPerformed) return;
    
using var scope = app.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
    
try
{
    logger.LogInformation("Initializing presence tracking - performing one-time cleanup");
    // Use Task.Run to execute the async code and wait for it to complete
    Task.Run(async () => await presenceTracker.CleanupDeadConnections()).GetAwaiter().GetResult();
    cleanupPerformed = true;
    logger.LogInformation("One-time presence cleanup completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error performing initial presence cleanup");
}

app.Lifetime.ApplicationStopping.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
    
    try
    {
        logger.LogInformation("Application stopping - unregistering presence instance");
        // Use Task.Run to execute the async code and wait for it to complete
        Task.Run(async () => await presenceTracker.UnregisterInstance()).GetAwaiter().GetResult();
        logger.LogInformation("Presence instance unregistered successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error unregistering presence instance on shutdown");
    }
});



app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authSchemes.Any(authscheme => authscheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = securityScheme;

            // Apply Bearer Authentication globally to all endpoints
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [securityScheme] = new List<string>() // No specific scopes
            });
        }
    }


    
}