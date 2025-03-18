namespace SignaliteWebAPI.Domain.DTOs.FriendRequests;

public class FriendRequestDTO
{
    public int SenderId { get; set; }
    public string SenderUsername { get; set; }
    public string RequestDate { get; set; }
}