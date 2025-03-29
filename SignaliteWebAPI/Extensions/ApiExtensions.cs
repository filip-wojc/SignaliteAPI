using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SignaliteWebAPI.Application.Features.Auth.Register;
using SignaliteWebAPI.Application.Features.Friends.GetFriendRequests;
using SignaliteWebAPI.Application.Features.Friends.GetUserFriends;
using SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;
using SignaliteWebAPI.Application.Features.Groups.CreateGroup;
using SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;
using SignaliteWebAPI.Application.Features.Users.AddProfilePhoto;
using SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;
using SignaliteWebAPI.Application.Features.Users.UpdateProfilePhoto;
using SignaliteWebAPI.Application.Features.Friends.GetUserFriends;
using SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;
using SignaliteWebAPI.Application.Features.Messages.SendMessage;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
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
        services.AddScoped<IValidator<ModifyUserCommand>, ModifyUserValidator>();
        services.AddScoped<IValidator<CreateGroupCommand>, CreateGroupValidator>();
        services.AddScoped<IValidator<UpdateGroupPhotoCommand>, UpdateGroupPhotoValidator>();
        services.AddScoped<IValidator<UpdateProfilePhotoCommand>, UpdateProfilePhotoValidator>();
        services.AddScoped<IValidator<UpdateBackgroundPhotoCommand>, UpdateBackgroundPhotoValidator>();
        services.AddScoped<IValidator<AddUserToGroupCommand>, AddUserToGroupValidator>();
        services.AddScoped<IValidator<SendMessageCommand>, SendMessageValidator>();
        services.AddSingleton(Log.Logger);

    }
}