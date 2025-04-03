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
        mapper.Map(request.ModifyUserDTO, user);
        await userRepository.ModifyUser(user);
        
        var friendsToMap = await friendsRepository.GetUserFriends(request.UserId);
        var usersToNotify = mapper.Map<List<UserBasicInfo>>(friendsToMap);
        await notificationsService.UserUpdated(user.Id, usersToNotify);
        // TODO: test
    }
}