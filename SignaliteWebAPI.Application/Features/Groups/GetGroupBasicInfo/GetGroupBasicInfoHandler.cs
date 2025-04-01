using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupBasicInfo;

public class GetGroupBasicInfoHandler(IGroupRepository groupRepository, IMapper mapper) : IRequestHandler<GetGroupBasicInfoQuery, GroupBasicInfoDTO>
{
    public async Task<GroupBasicInfoDTO> Handle(GetGroupBasicInfoQuery request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithPhoto(request.GroupId);
        var groupDto = mapper.Map<GroupBasicInfoDTO>(group);
        return groupDto;
    }
}