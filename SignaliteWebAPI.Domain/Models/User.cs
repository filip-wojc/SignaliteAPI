namespace SignaliteWebAPI.Domain.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string HashedPassword { get; set; }
    public string? PhotoUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public List<UserGroup> Groups { get; set; } = [];
    public List<UserFriend> Friends { get; set; } = [];
    public List<FriendRequest> FriendRequests { get; set; } = [];
}