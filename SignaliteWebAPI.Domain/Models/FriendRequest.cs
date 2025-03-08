namespace SignaliteWebAPI.Domain.Models;

public class FriendRequest
{
    public int Id { get; set; }
    public User Sender { get; set; }
    public int SenderId { get; set; }
    public User Recipient { get; set; }
    public int RecipientId { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
}