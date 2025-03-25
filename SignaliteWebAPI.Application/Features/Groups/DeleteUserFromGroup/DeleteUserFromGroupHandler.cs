using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteUserFromGroup;

public class DeleteUserFromGroupHandler(IGroupRepository groupRepository) : IRequestHandler<DeleteUserFromGroupCommand>
{
    public async Task Handle(DeleteUserFromGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithUsers(request.GroupId);
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
    }
}