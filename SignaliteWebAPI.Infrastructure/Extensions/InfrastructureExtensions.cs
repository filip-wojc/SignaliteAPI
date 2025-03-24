
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Interfaces.Services;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Repositories.Users;
using SignaliteWebAPI.Infrastructure.Services;
using SignaliteWebAPI.Infrastructure.SignalR;
using StackExchange.Redis;

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
        services.AddScoped<ITokenService, TokenService>();
        
        // redis config
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<PresenceTracker>();
        services.AddSignalR();
        services.AddSingleton<ConnectionCleanupService>(sp => 
        {
            var logger = sp.GetRequiredService<ILogger<ConnectionCleanupService>>();
            var serviceProvider = sp;
            return new ConnectionCleanupService(
                serviceProvider, 
                logger,
                cleanupInterval: TimeSpan.FromMinutes(1),
                heartbeatInterval: TimeSpan.FromSeconds(30),
                skipInitialCleanup: true); // This flag will prevent redundant initial cleanup
        });
        services.AddHostedService(sp => sp.GetRequiredService<ConnectionCleanupService>()); // grab the service created above
    }
    
    
}