using MediatR;

namespace SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;

public class SendFriendRequestCommand : IRequest
{
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
}