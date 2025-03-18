namespace SignaliteWebAPI.Domain.DTOs.FriendRequests;

public class SendFriendRequestDTO
{
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
}