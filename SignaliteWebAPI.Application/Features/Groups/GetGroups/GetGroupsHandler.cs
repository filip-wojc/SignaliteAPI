﻿using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroups;

public class GetGroupsHandler(
    IGroupRepository groupRepository, 
    IMapper mapper
    ) : IRequestHandler<GetGroupsQuery, List<GroupBasicInfoDTO>>
{
    public async Task<List<GroupBasicInfoDTO>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await groupRepository.GetUserGroupsWithPhoto(request.UserId);
        var groupDtos = mapper.Map<List<GroupBasicInfoDTO>>(groups, opt => 
            opt.Items["UserId"] = request.UserId
        );
        return groupDtos.OrderByDescending(g => g.LastMessageDate).ToList();
    }
}