using MediatR;

namespace SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;

public class AddUserToGroupCommand : IRequest
{
    public int GroupId { get; set; }
    public int OwnerId { get; set; }
    public int UserId { get; set; }
}