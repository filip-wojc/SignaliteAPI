using MediatR;
using Serilog;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteUserFromGroup;

public class DeleteUserFromGroupHandler(
    IGroupRepository groupRepository,
    IFriendsRepository friendsRepository, 
    IUnitOfWork unitOfWork, 
    ILogger logger
    ) : IRequestHandler<DeleteUserFromGroupCommand>
{
    public async Task Handle(DeleteUserFromGroupCommand request, CancellationToken cancellationToken)
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
        
        // TODO: GroupUpdated event
    }
    
}