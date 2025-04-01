using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupDetails;

public class GetGroupDetailsHandler(IGroupRepository groupRepository, IMapper mapper) : IRequestHandler<GetGroupDetailsQuery, GroupMembersDTO>
{
    public async Task<GroupMembersDTO> Handle(GetGroupDetailsQuery request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupDetails(request.GroupId);
        var groupDto = mapper.Map<GroupMembersDTO>(group);
        return groupDto;
    }
}