using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Behaviors;
using SignaliteWebAPI.Application.Features.User.AddUser;
using SignaliteWebAPI.Application.Features.Users.AddUser;
using SignaliteWebAPI.Application.Features.Users.SendFriendRequest;
using SignaliteWebAPI.Domain.Interfaces.Services;
using SignaliteWebAPI.Infrastructure.Services;
using SignaliteWebAPI.Middlewares;

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
        services.AddScoped<IValidator<AddUserCommand>, AddUserValidator>();
        services.AddScoped<IValidator<SendFriendRequestCommand>, SendFriendRequestValidator>();
    }
}