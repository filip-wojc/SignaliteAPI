namespace SignaliteWebAPI.Domain.DTOs.FriendRequests;

public class AcceptOrDeclineFriendRequestDTO
{
    public int UserId { get; set; }
    public int FriendRequestId { get; set; }
}