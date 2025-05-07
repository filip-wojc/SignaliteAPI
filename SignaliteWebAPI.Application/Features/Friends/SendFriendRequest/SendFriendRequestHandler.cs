using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;

public class SendFriendRequestHandler(
    IFriendsRepository repository, 
    IUserRepository userRepository,
    IMapper mapper, 
    INotificationsService notificationsService
    ) : IRequestHandler<SendFriendRequestCommand>
{
    public async Task Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendRequest = mapper.Map<FriendRequest>(request);
        var userFriends = await repository.GetAllUserFriends();
        var recipientUser = await userRepository.GetUserByUsername(request.RecipientUsername);
        friendRequest.RecipientId = recipientUser.Id;
        var isFriendAdded = userFriends.Any(uf => (uf.UserId == recipientUser.Id && uf.FriendId == request.SenderId) ||
                                                  (uf.UserId == request.SenderId && uf.FriendId == recipientUser.Id));

        if (isFriendAdded)
        {
            throw new BadRequestException("User already accepted friend request from sender");
        }
        await repository.SendFriendRequest(friendRequest);
        
        var friendRequestDto = mapper.Map<FriendRequestDTO>(friendRequest);
        await notificationsService.FriendRequest(friendRequestDto, recipientUser.Id);
    }
}