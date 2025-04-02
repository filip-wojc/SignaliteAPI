using MediatR;

namespace SignaliteWebAPI.Application.Features.Groups.ModifyGroupName;

public class ModifyGroupCommand : IRequest
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public string GroupName { get; set; }
}