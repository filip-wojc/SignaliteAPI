using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.ModifyUser;

public class ModifyUserHandler(
    IUserRepository userRepository, 
    IFriendsRepository friendsRepository,
    INotificationsService notificationsService,
    IMapper mapper
    ) : IRequestHandler<ModifyUserCommand>
{
    public async Task Handle(ModifyUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserById(request.UserId);
        var oldUsername = user.Username;
        
        mapper.Map(request.ModifyUserDTO, user);
        await userRepository.ModifyUser(user);
        
        var friendsToMap = await friendsRepository.GetUserFriends(request.UserId);
        var usersToNotify = mapper.Map<List<UserBasicInfo>>(friendsToMap);
        
        var userDto = mapper.Map<UserDTO>(user);
        await notificationsService.UserUpdated(userDto, oldUsername, usersToNotify);
    }
}