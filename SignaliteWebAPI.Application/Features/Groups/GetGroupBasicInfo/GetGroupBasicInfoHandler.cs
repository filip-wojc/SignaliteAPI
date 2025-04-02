using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupBasicInfo;

public class GetGroupBasicInfoHandler(
    IGroupRepository groupRepository, 
    IMapper mapper
    ) : IRequestHandler<GetGroupBasicInfoQuery, GroupBasicInfoDTO>
{
    public async Task<GroupBasicInfoDTO> Handle(GetGroupBasicInfoQuery request, CancellationToken cancellationToken)
    {
        var users = await groupRepository.GetUsersInGroup(request.GroupId);

        if (users.All(u => u.Id != request.UserId))
        {
            throw new ForbidException("You are not in this group.");
        }
        
        var group = await groupRepository.GetGroupWithPhoto(request.GroupId);
        var groupDto = mapper.Map<GroupBasicInfoDTO>(group);
        return groupDto;
    }
}