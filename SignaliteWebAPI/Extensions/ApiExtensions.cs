using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.User.AddUser;
using SignaliteWebAPI.Behaviors;
using SignaliteWebAPI.Middlewares;
using SignaliteWebAPI.Validators.Users;

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
        services.AddScoped<IValidator<AddUserCommand>, RegisterUserValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}