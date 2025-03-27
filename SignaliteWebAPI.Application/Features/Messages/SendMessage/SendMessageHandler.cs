using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Messages.SendMessage;

public class SendMessageHandler(
    IMessageRepository messageRepository,
    IAttachmentRepository attachmentRepository,
    IMediaService mediaService,
    IMapper mapper) : IRequestHandler<SendMessageCommand>
{
    public async Task Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var file = request.SendMessageDto.File;
        
        if (file == null && string.IsNullOrEmpty(request.SendMessageDto.Content))
        {
            throw new BadRequestException("Can't send empty message");
        }
        
        var message = mapper.Map<Message>(request.SendMessageDto);
        message.SenderId = request.SenderId;
        await messageRepository.AddMessage(message);

        if (file != null)
        {
            // TODO Check file type
            // TODO Add cloudinary - videos, gifs, sounds
            // TODO Add azure blob - other file types

            var uploadResult = mediaService.AddPhotoAsync(file);

            if (uploadResult.Result.Error != null)
                throw new CloudinaryException(uploadResult.Result.Error.Message);

            var attachment = new Attachment
            {
                Name = file.FileName,
                Type = file.ContentType,
                MessageId = message.Id,
                FileSize = file.Length / (1024.0 * 1024.0),
                Url = uploadResult.Result.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.Result.PublicId
            };
            
            await attachmentRepository.AddAttachment(attachment);
        }

        // TODO: Notification "MessageReceived"
    }
}