using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.ModifyGroupName;

public class ModifyGroupHandler(
    IGroupRepository groupRepository,
    INotificationsService notificationsService,
    IMapper mapper
    ) : IRequestHandler<ModifyGroupCommand>
{
    public async Task Handle(ModifyGroupCommand request, CancellationToken cancellationToken)
    {
        var groupToModify = await groupRepository.GetGroup(request.GroupId);
        if (groupToModify.OwnerId != request.UserId || groupToModify.IsPrivate)
        {
            throw new ForbidException("You are not allowed to modify this group.");
        }
        await groupRepository.ModifyGroupName(request.GroupName, groupToModify);
        
        var usersToMap = await groupRepository.GetUsersInGroup(request.GroupId);
        var members = mapper.Map<List<UserBasicInfo>>(usersToMap);
        var groupDto = mapper.Map<GroupBasicInfoDTO>(groupToModify);
        await notificationsService.GroupUpdated(groupDto,members, groupToModify.OwnerId);
        // TODO: TEST
    }
}