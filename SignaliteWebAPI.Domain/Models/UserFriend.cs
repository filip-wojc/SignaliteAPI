namespace SignaliteWebAPI.Domain.Models;

public class UserFriend
{
    public int Id { get; set; }
    public User User { get; set; }
    public required int UserId { get; set; }
    public User Friend { get; set; }
    public required int FriendId { get; set; }
    public DateTime FriendDate {get; set;} = DateTime.UtcNow;
}