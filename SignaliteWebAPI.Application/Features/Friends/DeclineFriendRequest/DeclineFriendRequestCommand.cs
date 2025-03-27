using MediatR;

namespace SignaliteWebAPI.Application.Features.Friends.DeclineFriendRequest;

public class DeclineFriendRequestCommand : IRequest
{
    public int UserId { get; set; }
    public int FriendRequestId { get; set; }
}