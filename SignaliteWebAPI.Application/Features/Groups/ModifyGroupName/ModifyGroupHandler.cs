using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.ModifyGroupName;

public class ModifyGroupHandler(IGroupRepository groupRepository) : IRequestHandler<ModifyGroupCommand>
{
    public async Task Handle(ModifyGroupCommand request, CancellationToken cancellationToken)
    {
        var groupToModify = await groupRepository.GetGroup(request.GroupId);
        if (groupToModify.OwnerId != request.UserId || groupToModify.IsPrivate)
        {
            throw new ForbidException("You are not allowed to modify this group.");
        }
        await groupRepository.ModifyGroupName(request.GroupName, groupToModify);
        
        // TODO: Group Modified Notification
    }
}