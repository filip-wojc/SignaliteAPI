using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;

public class SendFriendRequestHandler(
    IFriendsRepository repository, 
    IMapper mapper, 
    INotificationsService notificationsService
    ) : IRequestHandler<SendFriendRequestCommand>
{
    public async Task Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendRequest = mapper.Map<FriendRequest>(request);
        var userFriends = await repository.GetAllUserFriends();
        var isFriendAdded = userFriends.Any(uf => (uf.UserId == request.RecipientId && uf.FriendId == request.SenderId) ||
                                                  (uf.UserId == request.SenderId && uf.FriendId == request.RecipientId));

        if (isFriendAdded)
        {
            throw new BadRequestException("User already accepted friend request from sender");
        }
        await repository.SendFriendRequest(friendRequest);
        await notificationsService.SendFriendRequestNotification(
            request.RecipientId,
            request.SenderId,
            request.SenderUsername
        );
    }
}