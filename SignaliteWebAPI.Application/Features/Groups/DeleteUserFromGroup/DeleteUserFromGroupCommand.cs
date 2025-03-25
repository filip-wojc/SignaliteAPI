using MediatR;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteUserFromGroup;

public class DeleteUserFromGroupCommand : IRequest
{
    public int OwnerId { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
}