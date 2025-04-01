using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupDetails;

public class GetGroupDetailsQuery : IRequest<GroupMembersDTO>
{
    public int GroupId { get; set; }
}
