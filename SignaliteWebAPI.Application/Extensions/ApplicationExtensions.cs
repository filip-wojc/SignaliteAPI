using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
       services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
       services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
       services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    }
}