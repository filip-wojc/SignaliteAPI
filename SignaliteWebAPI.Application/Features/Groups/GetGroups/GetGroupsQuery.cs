using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroups;

public class GetGroupsQuery : IRequest<List<GroupBasicInfoDTO>>
{
    public int UserId { get; set; }
}