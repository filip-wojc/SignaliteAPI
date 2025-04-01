using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupMembers;

public class GetGroupMembersQuery : IRequest<GroupMembersDTO>
{
    public int GroupId { get; set; }
}