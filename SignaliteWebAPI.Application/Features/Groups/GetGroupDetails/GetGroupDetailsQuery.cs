using MediatR;
using SignaliteWebAPI.Domain.DTOs.Groups;

namespace SignaliteWebAPI.Application.Features.Groups.GetGroupDetails;

public class GetGroupDetailsQuery : IRequest<GroupDetailsDTO>
{
    public int GroupId { get; set; }
}
