using SignaliteWebAPI.Domain.Attachments;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Domain.DTOs.Messages;

public class MessageDTO
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime LastModification { get; set; }
    public AttachmentDTO? Attachment { get; set; }
    public UserBasicInfo Sender { get; set; }
}