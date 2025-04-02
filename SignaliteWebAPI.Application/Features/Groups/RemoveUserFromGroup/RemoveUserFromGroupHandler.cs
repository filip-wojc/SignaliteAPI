using AutoMapper;
using MediatR;
using Serilog;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.RemoveUserFromGroup;

public class RemoveUserFromGroupHandler(
    IGroupRepository groupRepository,
    IFriendsRepository friendsRepository, 
    INotificationsService notificationsService,
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    ILogger logger
    ) : IRequestHandler<RemoveUserFromGroupCommand>
{
    public async Task Handle(RemoveUserFromGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithUsers(request.GroupId);

        if (group.IsPrivate)
        {
            var userFriend = await friendsRepository.GetUserFriend(group.Users[0].UserId, group.Users[1].UserId);
            try
            {
                await unitOfWork.BeginTransactionAsync();
                friendsRepository.DeleteFriend(userFriend);
                groupRepository.DeleteGroup(group);
                await unitOfWork.CommitTransactionAsync();
                return;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to delete group");
                await unitOfWork.RollbackTransactionAsync();
                return;
            }
        }
        
        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbidException("You can't delete members from group unless you are the owner");
        }
        
        var userToDelete= group.Users.FirstOrDefault(u => u.UserId == request.UserId);
        if (userToDelete == null)
        {
            throw new NotFoundException("User not found in this group");
        }

        if (userToDelete.UserId == request.OwnerId)
        {
            throw new ForbidException("You can't delete yourself from group");
        }
        
        await groupRepository.DeleteUserFromGroup(group, request.UserId);
        var membersToMap = await groupRepository.GetUsersInGroup(request.GroupId);
        var members = mapper.Map<List<UserBasicInfo>>(membersToMap);
        await notificationsService.UserRemovedFromGroup(userToDelete.UserId, group.Id, members);
    }
    
}