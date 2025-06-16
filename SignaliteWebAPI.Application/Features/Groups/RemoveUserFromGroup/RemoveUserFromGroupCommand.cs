using MediatR;

namespace SignaliteWebAPI.Application.Features.Groups.RemoveUserFromGroup;

public class RemoveUserFromGroupCommand : IRequest
{
    public int OwnerId { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
}