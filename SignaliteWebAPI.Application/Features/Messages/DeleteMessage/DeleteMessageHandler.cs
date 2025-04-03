using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Messages.DeleteMessage;

public class DeleteMessageHandler(IMessageRepository messageRepository, IMediaService mediaService) : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetMessageWithAttachment(request.MessageId);
        if (message.SenderId != request.SenderId)
        {
            throw new ForbidException("You can't delete message that u didn't send");
        }
        await messageRepository.DeleteMessage(message);
        
        
        if (message.Attachment != null)
        {
            var attachment = message.Attachment;
            if (SupportedFileTypes.MimeTypes[FileType.Other].Contains(attachment.Type))
            {
                mediaService.DeleteStaticFile(attachment.Url);
            }
            else
            {
                await mediaService.DeleteMediaAsync(attachment.PublicId, attachment.Type);
            }
        }
        
        // TODO: MessageDeleted notification
    }
}