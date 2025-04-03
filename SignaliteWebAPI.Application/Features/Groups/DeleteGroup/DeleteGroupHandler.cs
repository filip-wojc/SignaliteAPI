using MediatR;
using Serilog;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteGroup;

public class DeleteGroupHandler(
    IGroupRepository groupRepository, 
    IFriendsRepository friendsRepository, 
    IUnitOfWork unitOfWork, 
    ILogger logger) : IRequestHandler<DeleteGroupCommand>
{
    public async Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
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
            throw new ForbidException("You can't delete group which you did not create");
        }
        groupRepository.DeleteGroup(group);
        await unitOfWork.SaveChangesAsync();
        
        // TODO: GroupDeleted notification
        
    }
}