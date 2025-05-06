using AutoMapper;
using MediatR;
using Serilog;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.DeleteGroup;

public class DeleteGroupHandler(
    IGroupRepository groupRepository, 
    IFriendsRepository friendsRepository,
    IMessageRepository messageRepository,
    INotificationsService notificationsService,
    IMediaService mediaService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger logger) : IRequestHandler<DeleteGroupCommand>
{
    public async Task Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithUsers(request.GroupId);
        var messages = await messageRepository.GetMessages(request.GroupId);

        if (group.IsPrivate)
        {
            var userFriend = await friendsRepository.GetUserFriend(group.Users[0].UserId, group.Users[1].UserId);
            try
            {
                await unitOfWork.BeginTransactionAsync();
                friendsRepository.DeleteFriend(userFriend);
                groupRepository.DeleteGroup(group);
                await unitOfWork.CommitTransactionAsync();
                await DeleteAttachmentFiles(messages);
                return;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to delete group");
                await unitOfWork.RollbackTransactionAsync();
                return;
            }
        }
        
        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbidException("You can't delete group which you did not create");
        }
        var usersToMap = await groupRepository.GetUsersInGroup(request.GroupId);
        groupRepository.DeleteGroup(group);
        await unitOfWork.SaveChangesAsync();
        await DeleteAttachmentFiles(messages);
        
        var members = mapper.Map<List<UserBasicInfo>>(usersToMap);
        await notificationsService.GroupDeleted(request.GroupId, request.OwnerId, members);
        
    }

    private async Task DeleteAttachmentFiles(List<Message> messages)
    {
        foreach (var attachment in messages.Where(m => m.Attachment != null).Select(m => m.Attachment))
        {
            if (SupportedFileTypes.MimeTypes[FileType.Other].Contains(attachment.Type))
            {
                mediaService.DeleteStaticFile(attachment.Url);
            }
            else
            {
                await mediaService.DeleteMediaAsync(attachment.PublicId, attachment.Type);
            }
        }
    }
}