namespace SignaliteWebAPI.Domain.Attachments;

public class AttachmentDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required double FileSize { get; set; }
    public required string Type { get; set; }
    public required string Url { get; set; }
}