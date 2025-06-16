namespace SignaliteWebAPI.Domain.DTOs.FriendRequests;

public class FriendRequestDTO
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; }
    public string ProfilePhotoUrl { get; set; }
    public string RequestDate { get; set; }
}