using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestCommand : IRequest
{
    public int UserId { get; set; }
    public int FriendRequestId { get; set; }
}