using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.SendFriendRequest;

public class SendFriendRequestCommand : IRequest
{
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
}