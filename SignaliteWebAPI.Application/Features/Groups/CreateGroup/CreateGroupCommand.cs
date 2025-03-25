using MediatR;

namespace SignaliteWebAPI.Application.Features.Groups.CreateGroup;

public class CreateGroupCommand : IRequest
{
    public string Name { get; set; }
    public int OwnerId { get; set; }
}