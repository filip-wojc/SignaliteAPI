using MediatR;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteGroup;

public class DeleteGroupCommand : IRequest
{
    public int GroupId { get; set; }
    public int OwnerId { get; set; }
}