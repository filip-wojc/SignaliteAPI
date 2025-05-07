using MediatR;

namespace SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;

public class SendFriendRequestCommand : IRequest
{
    public int SenderId { get; set; }
    public required string SenderUsername { get; set; }
    public required string RecipientUsername { get; set; }
}