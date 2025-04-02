using FluentValidation;
using SignaliteWebAPI.Application.Features.Auth.Register;
using SignaliteWebAPI.Application.Features.Friends.GetFriendRequests;
using SignaliteWebAPI.Application.Features.Friends.GetUserFriends;
using SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;
using SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;
using SignaliteWebAPI.Application.Features.Groups.CreateGroup;
using SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;
using SignaliteWebAPI.Application.Features.Messages.GetMessageThread;
using SignaliteWebAPI.Application.Features.Messages.SendMessage;
using SignaliteWebAPI.Application.Features.Users.AddProfilePhoto;
using SignaliteWebAPI.Application.Features.Users.ChangePassword;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
using SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;
using SignaliteWebAPI.Application.Features.Users.UpdateProfilePhoto;

namespace SignaliteWebAPI.Extensions;

public static class ValidatorExtensions
{
    public static void AddValidatorExtensions(this IServiceCollection services)
    {
        services.AddScoped<IValidator<RegisterCommand>, RegisterValidator>();
        services.AddScoped<IValidator<SendFriendRequestCommand>, SendFriendRequestValidator>();
        services.AddScoped<IValidator<GetFriendRequestsQuery>, GetFriendRequestsValidator>(); 
        services.AddScoped<IValidator<GetUserFriendsQuery>, GetUserFriendsValidator>();
        services.AddScoped<IValidator<CreateGroupCommand>, CreateGroupValidator>();
        services.AddScoped<IValidator<UpdateGroupPhotoCommand>, UpdateGroupPhotoValidator>();
        services.AddScoped<IValidator<UpdateProfilePhotoCommand>, UpdateProfilePhotoValidator>();
        services.AddScoped<IValidator<ModifyUserCommand>, ModifyUserValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordValidator>();
        services.AddScoped<IValidator<UpdateBackgroundPhotoCommand>, UpdateBackgroundPhotoValidator>();
        services.AddScoped<IValidator<AddUserToGroupCommand>, AddUserToGroupValidator>();
        services.AddScoped<IValidator<SendMessageCommand>, SendMessageValidator>();
        services.AddScoped<IValidator<GetMessageThreadQuery>, GetMessageThreadValidator>();
    }
}