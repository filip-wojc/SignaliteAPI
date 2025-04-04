using AutoMapper;
using MediatR;
using Serilog;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteGroup;

public class DeleteGroupHandler(
    IGroupRepository groupRepository, 
    IFriendsRepository friendsRepository, 
    INotificationsService notificationsService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
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
        var usersToMap = await groupRepository.GetUsersInGroup(request.GroupId);
        groupRepository.DeleteGroup(group);
        await unitOfWork.SaveChangesAsync();
        
        var members = mapper.Map<List<UserBasicInfo>>(usersToMap);
        await notificationsService.GroupDeleted(request.GroupId, request.OwnerId, members);
        
    }
}