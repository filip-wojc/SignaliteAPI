using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestHandler(IFriendsRepository friendsRepository) : IRequestHandler<AcceptFriendRequestCommand>
{
    public async Task Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendsRequest = await friendsRepository.GetFriendRequests(request.AcceptFriendRequestDto.UserId);
        
        var requestToAccept = friendsRequest.FirstOrDefault(fr => fr.Id == request.AcceptFriendRequestDto.FriendRequestId);
        if (requestToAccept == null)
        {
            throw new NotFoundException("User doesn't have a friend request with given id");
        }
        
        var userFriend = new UserFriend
        {
            UserId = requestToAccept.RecipientId,
            FriendId = requestToAccept.SenderId
        };
        
        await friendsRepository.AddFriend(userFriend);
        await friendsRepository.DeleteFriendRequest(requestToAccept);
    }
}