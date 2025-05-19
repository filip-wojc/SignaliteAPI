using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Messages.DeleteMessage;

public class DeleteMessageHandler(
    IMessageRepository messageRepository,
    IGroupRepository groupRepository,
    IMediaService mediaService,
    INotificationsService notificationsService,
    IMapper mapper) : IRequestHandler<DeleteMessageCommand>
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
        
        var lastMessage = await messageRepository.GetLastMessage(message.GroupId);
        var lastMessageDto = mapper.Map<MessageDTO>(lastMessage);
        var usersToMap = await groupRepository.GetUsersInGroup(message.GroupId);
        var members = mapper.Map<List<UserBasicInfo>>(usersToMap);
        await notificationsService.MessageDeleted(message.GroupId, message.Id, message.SenderId, lastMessageDto, members);
    }
}