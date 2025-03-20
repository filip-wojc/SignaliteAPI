using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestCommand : IRequest
{
    public FriendRequestReplyDTO AcceptFriendRequestReplyDto { get; set; }
}