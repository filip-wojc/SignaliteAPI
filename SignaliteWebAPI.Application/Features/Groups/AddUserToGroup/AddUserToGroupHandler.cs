using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;

public class AddUserToGroupHandler(
    IGroupRepository groupRepository,
    INotificationsService notificationsService,
    IMapper mapper
    ) : IRequestHandler<AddUserToGroupCommand>
{
    public async Task Handle(AddUserToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithUsers(request.GroupId);
        
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
        
        await groupRepository.AddUserToGroup(userGroup);
        var groupInfo = mapper.Map<GroupBasicInfo>(group);
        await notificationsService.SendAddedToGroupNotification(request.UserId, request.OwnerId, groupInfo); ;
        

        // TODO: UserAddedToGroup notification
    }
}