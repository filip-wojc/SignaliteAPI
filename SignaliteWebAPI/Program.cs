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
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var presenceTracker = services.GetRequiredService<PresenceTracker>();
    await presenceTracker.CleanupDeadConnections();
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
    
    try
    {
        logger.LogInformation("Application started - initializing presence tracking");
        
        // Initial cleanup of dead connections from previous runs
        presenceTracker.CleanupDeadConnections();
        
        logger.LogInformation("Presence tracking initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing presence tracking on startup");
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
    
    try
    {
        logger.LogInformation("Application stopping - unregistering presence instance");
        presenceTracker.UnregisterInstance();
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
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
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