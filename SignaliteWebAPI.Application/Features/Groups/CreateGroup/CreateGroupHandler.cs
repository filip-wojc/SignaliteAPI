using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.CreateGroup;

public class CreateGroupHandler(IGroupRepository groupRepository, IMapper mapper) : IRequestHandler<CreateGroupCommand>
{
    public async Task Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = mapper.Map<CreateGroupCommand, Group>(request);
        await groupRepository.CreateGroup(group);
        var userGroup = new UserGroup
        {
            GroupId = group.Id,
            UserId = request.OwnerId
        };
        await groupRepository.AddUserToGroup(userGroup);
    }
}