using MediatR;

namespace SignaliteWebAPI.Application.Features.Messages.DeleteAttachment;

public class DeleteAttachmentCommand : IRequest
{
    public int MessageId { get; set; }
    public int SenderId { get; set; }
}