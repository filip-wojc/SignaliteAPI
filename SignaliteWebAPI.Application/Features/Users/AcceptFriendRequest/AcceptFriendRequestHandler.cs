using MediatR;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestHandler(IFriendsRepository friendsRepository) : IRequestHandler<AcceptFriendRequestCommand>
{
    public async Task Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendRequest = await friendsRepository.GetFriendRequest(request.FriendRequestId);
        var userFriend = new UserFriend
        {
            UserId = friendRequest.RecipientId,
            FriendId = friendRequest.SenderId
        };
        await friendsRepository.AddFriend(userFriend);
        await friendsRepository.DeleteFriendRequest(friendRequest);
    }
}