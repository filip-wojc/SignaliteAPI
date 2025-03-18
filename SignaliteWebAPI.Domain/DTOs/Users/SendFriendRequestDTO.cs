namespace SignaliteWebAPI.Domain.DTOs.Users;

public class SendFriendRequestDTO
{
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
}