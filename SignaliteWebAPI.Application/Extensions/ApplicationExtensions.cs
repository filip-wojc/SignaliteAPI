using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SignaliteWebAPI.Application.Behaviors;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
       services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
       services.AddMediatR(cfg =>
       {
           cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
           cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
       });
       services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    }
}