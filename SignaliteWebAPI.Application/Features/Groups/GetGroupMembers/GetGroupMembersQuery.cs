using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupMembers;

public class GetGroupMembersQuery : IRequest<GroupMembersDTO>
{
    public int UserId { get; set; }
    public int GroupId { get; set; }
}