using MediatR;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Friends.DeclineFriendRequest;

public class DeclineFriendRequestHandler(
    IFriendsRepository friendsRepository, 
    IUnitOfWork unitOfWork
): IRequestHandler<DeclineFriendRequestCommand>
{
    public async Task Handle(DeclineFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendsRequest = await friendsRepository.GetFriendRequests(request.UserId);
        var requestToDecline = friendsRequest.FirstOrDefault(fr => fr.Id == request.FriendRequestId);
        if (requestToDecline == null)
        {
            throw new NotFoundException("User doesn't have a friend request with given id");
        }
        friendsRepository.DeleteFriendRequest(requestToDecline);
        await unitOfWork.SaveChangesAsync();
    }
}