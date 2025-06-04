using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Messages.ModifyMessage;

public class ModifyMessageHandler(
    IMessageRepository messageRepository,
    IGroupRepository groupRepository,
    INotificationsService notificationsService,
    IMapper mapper
    ) : IRequestHandler<ModifyMessageCommand>
{
    public async Task Handle(ModifyMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetMessage(request.MessageId);
        if (message.SenderId != request.SenderId)
        {
            throw new ForbidException("You are not allowed to modify this message.");
        }

        await messageRepository.ModifyMessage(request.MessageContent, message);
        var usersToMap = await groupRepository.GetUsersInGroup(message.GroupId);
        var members = mapper.Map<List<UserBasicInfo>>(usersToMap);
        var messageDto = mapper.Map<MessageDTO>(message);
        var lastMessageInGroup = await messageRepository.GetLastMessage(message.GroupId);

        if (lastMessageInGroup?.Id == messageDto.Id)
        {
            await notificationsService.MessageModified(messageDto, message.GroupId, members, true);
        }
        else
        {
            await notificationsService.MessageModified(messageDto, message.GroupId, members, false);
        }
            
        
    }
    
    
}