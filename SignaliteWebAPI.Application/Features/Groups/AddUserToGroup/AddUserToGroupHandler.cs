using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;


public class AddUserToGroupHandler(
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    INotificationsService notificationsService,
    IUnitOfWork unitOfWork,
    IMapper mapper
    ) : IRequestHandler<AddUserToGroupCommand>
{
    public async Task Handle(AddUserToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithUsers(request.GroupId);

        if (group.IsPrivate)
        {
            throw new ForbidException("You can't add user to private group");
        }
        
        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbidException("You can't add members to group unless you are the owner");
        }
        
        var user = group.Users.FirstOrDefault(u => u.UserId == request.UserId);
        if (user != null)
        {
            throw new ForbidException("User is already in this group");
        }

        var userGroup = new UserGroup
        {
            GroupId = request.GroupId,
            UserId = request.UserId
        };

        try
        {
            await unitOfWork.BeginTransactionAsync();
            await groupRepository.AddUserToGroup(userGroup);
            var groupInfo = mapper.Map<GroupBasicInfoDTO>(group);
            var membersToMap = await groupRepository.GetUsersInGroup(request.GroupId);
            var members = mapper.Map<List<UserBasicInfo>>(membersToMap);
            var addedUser = await userRepository.GetUserById(request.UserId);
            var addedUserInfo = mapper.Map<UserBasicInfo>(addedUser);
            // wait with notifications until everything before finishes
            await notificationsService.SendAddedToGroupNotification(request.UserId, request.OwnerId, groupInfo);
            await notificationsService.UserAddedToGroup(addedUserInfo, members);
            await unitOfWork.CommitTransactionAsync(); // wait with commiting until everything else finishes

        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(); // rollback the insert 
            throw; 
        }
        
        
    }
}