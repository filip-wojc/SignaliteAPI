namespace SignaliteWebAPI.Domain.Models;

public class Message
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime DateSent { get; set; } = DateTime.UtcNow;
    public DateTime? DateModified { get; set; }
    public User Sender { get; set; }
    public required int SenderId { get; set; }
    public Group Group { get; set; }
    public required int GroupId { get; set; }
    public Attachment Attachment { get; set; }
}