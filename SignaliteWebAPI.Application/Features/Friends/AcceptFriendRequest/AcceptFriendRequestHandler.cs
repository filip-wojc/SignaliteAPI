using MediatR;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Friends.AcceptFriendRequest;

public class AcceptFriendRequestHandler(IFriendsRepository friendsRepository, IGroupRepository groupRepository, IUserRepository userRepository) : IRequestHandler<AcceptFriendRequestCommand>
{
    public async Task Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendsRequest = await friendsRepository.GetFriendRequests(request.UserId);
        
        var requestToAccept = friendsRequest.FirstOrDefault(fr => fr.Id == request.FriendRequestId);
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
        
        var user = await userRepository.GetUserById(requestToAccept.RecipientId);
        var friend = await userRepository.GetUserById(requestToAccept.SenderId);

        var group = new Group
        {
            Name = $"{user!.Username}, {friend!.Username}",
            OwnerId = user.Id
        };
        
        await groupRepository.CreateGroup(group);

        var userGroup = new UserGroup
        {
            UserId = user.Id,
            GroupId = group.Id
        };
        
        await groupRepository.AddUserToGroup(userGroup);
        
        // TODO: Notification "GroupCreated"
    }
}