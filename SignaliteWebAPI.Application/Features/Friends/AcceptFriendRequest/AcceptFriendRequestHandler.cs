using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Friends.AcceptFriendRequest;

public class AcceptFriendRequestHandler(
    IFriendsRepository friendsRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    INotificationsService notificationsService
    ): IRequestHandler<AcceptFriendRequestCommand>
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

        var user = await userRepository.GetUserById(requestToAccept.RecipientId);
        var friend = await userRepository.GetUserById(requestToAccept.SenderId);

        var group = new Group
        {
            Name = $"{user.Username}, {friend.Username}",
            OwnerId = user.Id,
            IsPrivate = true
        };

        // Creating group outside of transaction scope to get groupId
        await groupRepository.CreateGroup(group);
        await unitOfWork.SaveChangesAsync();

        try
        {
            await unitOfWork.BeginTransactionAsync();
            await friendsRepository.AddFriend(userFriend);
            friendsRepository.DeleteFriendRequest(requestToAccept);

            var userGroup = new UserGroup
            {
                UserId = user.Id,
                GroupId = group.Id
            };
            var userGroup2 = new UserGroup
            {
                UserId = friend.Id,
                GroupId = group.Id
            };
            await groupRepository.AddUserToGroup(userGroup);
            await groupRepository.AddUserToGroup(userGroup2);
            var createdGroup = await groupRepository.GetGroupWithPhoto(group.Id);
            var groupDto = mapper.Map<GroupBasicInfoDTO>(createdGroup, opt => 
                opt.Items["UserId"] = request.UserId);
            await notificationsService.FriendRequestAccepted(groupDto, requestToAccept.SenderId);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();

            // Deleting group if transaction failed
            groupRepository.DeleteGroup(group);
            await unitOfWork.SaveChangesAsync();
        }
    }
}