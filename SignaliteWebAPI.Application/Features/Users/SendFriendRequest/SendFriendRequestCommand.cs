using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.SendFriendRequest;

public class SendFriendRequestCommand : IRequest
{
    public SendFriendRequestDTO SendFriendRequestDTO { get; set; }
}