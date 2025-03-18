namespace SignaliteWebAPI.Domain.DTOs.FriendRequests;

public class GetFriendRequestsDTO
{
    public int SenderId { get; set; }
    public DateTime RequestDate { get; set; }
}