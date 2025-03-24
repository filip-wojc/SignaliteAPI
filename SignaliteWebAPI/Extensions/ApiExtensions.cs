using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Auth.Register;
using SignaliteWebAPI.Application.Features.Friends.GetUserFriends;
using SignaliteWebAPI.Application.Features.Users.GetFriendRequests;
using SignaliteWebAPI.Application.Features.Users.SendFriendRequest;
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
        services.AddScoped<IValidator<RegisterCommand>, RegisterValidator>();
        services.AddScoped<IValidator<SendFriendRequestCommand>, SendFriendRequestValidator>();
        services.AddScoped<IValidator<GetFriendRequestsQuery>, GetFriendRequestsValidator>();
        services.AddScoped<IValidator<GetUserFriendsQuery>, GetUserFriendsValidator>();
    }
}