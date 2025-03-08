namespace SignaliteWebAPI.Domain.Models;

public class MessageRead
{
    public int Id { get; set; }
    public Message Message { get; set; }
    public required int MessageId { get; set; }
    public User ReadBy { get; set; }
    public required int ReadById { get; set; }
}