using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;
using SignaliteWebAPI.Infrastructure.SignalR;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Application.Features.Messages.SendMessage;

public class SendMessageHandler(
    IMessageRepository messageRepository,
    IGroupRepository groupRepository,
    IAttachmentRepository attachmentRepository,
    IMediaService mediaService,
    INotificationsService notificationsService,
    IUnitOfWork unitOfWork,
    IMapper mapper
    ): IRequestHandler<SendMessageCommand>
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

        // check if file is okay to upload, otherwise throw exception to cancel operation
        if (file != null)
        {
            if (file.Length > SupportedFileTypes.MaxFileSize)
            {
                throw new BadRequestException($"File size exceeds the maximum limit of 50MB");
            }

            if (!SupportedFileTypes.IsSupportedMimeType(file.ContentType))
            {
                throw new BadRequestException($"Unsupported file type: {file.ContentType}");
            }
        }
        
        // TODO: ADD Other file types STATIC FILE STORAGE
        string? uploadedPublicId = null;
        string? mimeType = null;

        try
        {
            // beginning of the transaction
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            // add the message to get an ID
            await messageRepository.AddMessage(message);
            await unitOfWork.SaveChangesAsync(cancellationToken); // IMPORTANT: needed to get id for message

            // try to create attachment
            if (file != null)
            {
                var (url, publicId, type, fileName) = await UploadFileBasedOnType(file);
                uploadedPublicId = publicId;
                mimeType = type;

                var attachment = new Attachment
                {
                    Name = fileName,
                    Type = mimeType,
                    MessageId = message.Id,
                    FileSize = Math.Round(file.Length / (1024.0 * 1024.0), 2),
                    Url = url,
                    PublicId = publicId
                };

                await attachmentRepository.AddAttachment(attachment);
            }

            // if we got here, everything succeeded, so commit the transaction
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var usersToMap = await groupRepository.GetUsersInGroup(request.SendMessageDto.GroupId);
            var usersInGroup = mapper.Map<List<UserBasicInfo>>(usersToMap);
            var messageDto = mapper.Map<MessageDTO>(message);
            await notificationsService.MessageReceived(usersInGroup, messageDto);
            
        }
        catch (Exception)
        {
            try
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (InvalidOperationException) 
            {
                // Transaction already rolled back or disposed, ignore
            }
            // Clean up Cloudinary resource if it was uploaded
            if (string.IsNullOrEmpty(uploadedPublicId)) throw; // throw to exception handler
            try
            {
                await mediaService.DeleteMediaAsync(uploadedPublicId, mimeType ?? "image/jpeg");
            }
            catch
            {
                // Log but continue - don't throw another exception during cleanup 
            }
        }
    }

    // use the correct function based on the mime type
    private async Task<(string Url, string PublicId, string MimeType,string fileName)> UploadFileBasedOnType(IFormFile file)
    {
        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(file.ContentType);

        switch (fileType)
        {
            case FileType.Image:
                var imageResult = await mediaService.AddPhotoAsync(file);
                return (imageResult.SecureUrl.AbsoluteUri, imageResult.PublicId, file.ContentType, file.FileName);

            case FileType.Video:
                var videoResult = await mediaService.AddVideoAsync(file);
                return (videoResult.SecureUrl.AbsoluteUri, videoResult.PublicId, file.ContentType, file.FileName);

            case FileType.Audio:
                var audioResult = await mediaService.AddAudioAsync(file);
                return (audioResult.SecureUrl.AbsoluteUri, audioResult.PublicId, file.ContentType, file.FileName);

            case FileType.Other:
                var staticResult = await mediaService.AddStaticFile(file);
                return (staticResult.Url, staticResult.PublicId, staticResult.Type, staticResult.FileName);

            default:
                throw new BadRequestException($"Unsupported file type: {file.ContentType}");
        }
    }

}