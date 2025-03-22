using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Helpers;
using SignaliteWebAPI.Infrastructure.Interfaces;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;
using SignaliteWebAPI.Infrastructure.Repositories.Users;
using SignaliteWebAPI.Infrastructure.Services;
using SignaliteWebAPI.Infrastructure.Services.Media;

namespace SignaliteWebAPI.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SignaliteDbContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFriendsRepository, FriendsRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings")); // fill CloudinarySettings class with fields from appsettings
        services.AddScoped<IMediaService, MediaService>();
    }
}