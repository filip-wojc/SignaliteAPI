using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Messages.DeleteAttachment;

public class DeleteAttachmentHandler(
    IAttachmentRepository attachmentRepository,
    IMessageRepository messageRepository,
    IMediaService mediaService) : IRequestHandler<DeleteAttachmentCommand>
{
    public async Task Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetMessageWithAttachment(request.MessageId);
        if (message.SenderId != request.SenderId)
        {
            throw new ForbidException("You cant delete this attachment.");
        }

        var attachment = message.Attachment;
        
        if (attachment == null)
        {
            throw new BadRequestException("This message doesnt have an attachment.");
        }
        
        await attachmentRepository.DeleteAttachment(attachment);
        if (SupportedFileTypes.MimeTypes[FileType.Other].Contains(attachment.Type))
        {
            mediaService.DeleteStaticFile(attachment.Url);
        }
        else
        {
            await mediaService.DeleteMediaAsync(attachment.PublicId, attachment.Type);
        }
        // TODO: AttachmentRemoved notification
    }
}