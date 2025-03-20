using MediatR;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Exceptions;

namespace SignaliteWebAPI.Application.Features.Users.DeclineFriendRequest;

public class DeclineFriendRequestHandler(IFriendsRepository friendsRepository) : IRequestHandler<DeclineFriendRequestCommand>
{
    public async Task Handle(DeclineFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendsRequest = await friendsRepository.GetFriendRequests(request.DeclineFriendRequestReplyDto.UserId);
        var requestToDecline = friendsRequest.FirstOrDefault(fr => fr.Id == request.DeclineFriendRequestReplyDto.FriendRequestId);
        if (requestToDecline == null)
        {
            throw new NotFoundException("User doesn't have a friend request with given id");
        }
        await friendsRepository.DeleteFriendRequest(requestToDecline);
    }
}