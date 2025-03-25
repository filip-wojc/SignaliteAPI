
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Helpers;
using SignaliteWebAPI.Infrastructure.Interfaces;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;
using SignaliteWebAPI.Infrastructure.Repositories;
using SignaliteWebAPI.Infrastructure.Services;
using SignaliteWebAPI.Infrastructure.SignalR;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;
using SignaliteWebAPI.Infrastructure.Services.Media;


namespace SignaliteWebAPI.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SignaliteDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
        });
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFriendsRepository, FriendsRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<ITokenService, TokenService>();
        
        // redis config
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<PresenceTracker>();
        services.AddSignalR();
        services.AddSingleton<ConnectionCleanupService>(sp => 
        {
            var logger = sp.GetRequiredService<ILogger>();
            var serviceProvider = sp;
            return new ConnectionCleanupService(
                serviceProvider, 
                logger,
                cleanupInterval: TimeSpan.FromMinutes(1),
                heartbeatInterval: TimeSpan.FromSeconds(30),
                skipInitialCleanup: true); // This flag will prevent redundant initial cleanup
        });
        services.AddHostedService(sp => sp.GetRequiredService<ConnectionCleanupService>()); // grab the service created above

        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings")); // fill CloudinarySettings class with fields from appsettings
        services.AddScoped<IMediaService, MediaService>();
    }     
}