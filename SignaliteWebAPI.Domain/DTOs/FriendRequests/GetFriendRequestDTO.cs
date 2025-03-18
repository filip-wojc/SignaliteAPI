namespace SignaliteWebAPI.Domain.DTOs.FriendRequests;

public class GetFriendRequestDTO
{
    public int SenderId { get; set; }
    public DateTime RequestDate { get; set; }
}