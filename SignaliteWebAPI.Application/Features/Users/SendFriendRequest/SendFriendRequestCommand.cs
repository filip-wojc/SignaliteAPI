using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Users.SendFriendRequest;

public class SendFriendRequestCommand : IRequest
{
    public SendFriendRequestDTO SendFriendRequestDTO { get; set; }
}