using MediatR;

namespace SignaliteWebAPI.Application.Features.Friends.AcceptFriendRequest;

public class AcceptFriendRequestCommand : IRequest
{
    public int UserId { get; set; }
    public int FriendRequestId { get; set; }
}