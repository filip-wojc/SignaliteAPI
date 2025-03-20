using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.DeclineFriendRequest;

public class DeclineFriendRequestCommand : IRequest
{
    public FriendRequestReplyDTO DeclineFriendRequestReplyDto { get; set; }
}