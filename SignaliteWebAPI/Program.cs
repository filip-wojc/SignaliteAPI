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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Helpers;
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
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(10);
        o.QueueLimit = 0;
    });
    o.AddFixedWindowLimiter("file-upload", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(5);
        o.QueueLimit = 2;  
    });
});
string currDir = Directory.GetCurrentDirectory();
string staticFilesDirectory = builder.Configuration.Get<StaticFilesConfig>()?.Directory ?? "wwwroot/Attachments";
string attachmentsPath = Path.Combine(currDir, staticFilesDirectory);

builder.Configuration.AddEnvironmentVariables();


if (!Directory.Exists(attachmentsPath))
{
    Directory.CreateDirectory(attachmentsPath);
}

builder.Services.AddSingleton(new AttachmentPath
{
    BaseDirectory = currDir,
    AttachmentsDirectory = attachmentsPath,
    RequestUrl = builder.Configuration.Get<StaticFilesConfig>()?.RequestUrl ?? "http://localhost:5000/Attachments",
});

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
using var scope = app.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();


    try
    {
        logger.LogInformation("Applying database migrations...");
        var context = scope.ServiceProvider.GetRequiredService<SignaliteDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(attachmentsPath),
    RequestPath = "/Attachments"
});

app.ConfigureSerilogHttpLogging(); // extension
app.UseHttpLogging(); // Logs request & response headers, body, etc.
app.UseExceptionHandler(_ => { });
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials() // allow credentials to make passing the token to SignalR hubs possible
    .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5026")); // must be declared before MapControllers() to work
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<NotificationsHub>("hubs/notifications");
app.MapHub<SignalingHub>("hubs/signaling");


bool cleanupPerformed = false;

if (cleanupPerformed) return;
    

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