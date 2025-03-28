using AutoMapper;
using CloudinaryDotNet.Actions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Application.Features.Messages.SendMessage;

public class SendMessageHandler(
    IMessageRepository messageRepository,
    IAttachmentRepository attachmentRepository,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger logger) : IRequestHandler<SendMessageCommand>
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
                // upload to Cloudinary
                var uploadResult = await UploadFileBasedOnType(file);

                if (uploadResult == null || uploadResult.Error != null)
                {
                    // rollback message transaction if something went wrong with cloudinary upload
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw new CloudinaryException(uploadResult?.Error?.Message ??
                                                  "Failed to upload file to Cloudinary");
                }
                
                uploadedPublicId = uploadResult.PublicId;
                mimeType = file.ContentType;
                // Create attachment entity
                var attachment = new Attachment
                {
                    Name = file.FileName,
                    Type = file.ContentType,
                    MessageId = message.Id,
                    FileSize = file.Length / (1024.0 * 1024.0),
                    Url = uploadResult.SecureUrl.AbsoluteUri,
                    PublicId = uploadResult.PublicId
                };

                // add attachment to db
                await attachmentRepository.AddAttachment(attachment);
            }

            // if we got here, everything succeeded, so commit the transaction
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // TODO: Notification "MessageReceived"
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


        // TODO: Notification "MessageReceived"
    }

    // use the correct function based on the mime type
    private async Task<UploadResult> UploadFileBasedOnType(IFormFile file)
    {
        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(file.ContentType);

        return fileType switch
        {
            FileType.Image => await mediaService.AddPhotoAsync(file),
            FileType.Video => await mediaService.AddVideoAsync(file),
            FileType.Audio => await mediaService.AddAudioAsync(file),
            // FileType.Other => await mediaService.AddOtherAsync(file), // use other service
            _ => throw new BadRequestException($"Unsupported file type: {file.ContentType}")
        };
    }
}