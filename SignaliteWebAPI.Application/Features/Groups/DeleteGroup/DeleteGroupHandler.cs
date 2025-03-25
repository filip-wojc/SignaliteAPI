using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteGroup;

public class DeleteGroupHandler(IGroupRepository groupRepository) : IRequestHandler<DeleteGroupCommand>
{
    public async Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithUsers(request.GroupId);
        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbidException("You can't delete group which you did not create");
        }
        await groupRepository.DeleteGroup(group);
    }
}