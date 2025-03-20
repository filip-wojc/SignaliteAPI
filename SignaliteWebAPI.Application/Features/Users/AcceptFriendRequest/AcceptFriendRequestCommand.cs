using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestCommand : IRequest
{
    public AcceptOrDeclineFriendRequestDTO AcceptFriendRequestDto { get; set; }
}