using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupMembers;

public class GetGroupMembersHandler(IGroupRepository groupRepository, IMapper mapper) : IRequestHandler<GetGroupMembersQuery, GroupMembersDTO>
{
    public async Task<GroupMembersDTO> Handle(GetGroupMembersQuery request, CancellationToken cancellationToken)
    {
        var groupWithMembers = await groupRepository.GetGroupMembers(request.GroupId);
        return mapper.Map<GroupMembersDTO>(groupWithMembers);
    }
}