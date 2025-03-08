namespace SignaliteWebAPI.Domain.Models;

public class Attachment
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Url { get; set; }
    public Message Message { get; set; }
    public required int MessageId { get; set; }
}