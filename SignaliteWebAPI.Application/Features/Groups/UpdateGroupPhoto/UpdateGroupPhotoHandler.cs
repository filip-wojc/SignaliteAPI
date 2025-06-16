using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;

public class UpdateGroupPhotoHandler(
    IGroupRepository groupRepository,
    IPhotoRepository photoRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IMediaService mediaService,
    INotificationsService notificationsService
    ) : IRequestHandler<UpdateGroupPhotoCommand>
{
    public async Task Handle(UpdateGroupPhotoCommand request, CancellationToken cancellationToken)
    {
        var group = await groupRepository.GetGroupWithPhoto(request.GroupId);
        if (group.OwnerId != request.OwnerId || group.IsPrivate)
        {
            throw new ForbidException("You are not allowed to update the group photo");
        }
        
        var uploadResult = await mediaService.AddPhotoAsync(request.Photo);
        if (uploadResult.Error != null)
            throw new CloudinaryException(uploadResult.Error.Message);
        
        var photo = new Photo
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            UserId = group.OwnerId
        };
        
        try
        {

            await unitOfWork.BeginTransactionAsync(cancellationToken);
        
            // delete old photo if exists
            if (group.Photo != null)
            {
                var photoId = group.Photo.Id;
                await mediaService.DeleteMediaAsync(group.Photo.PublicId);
                await photoRepository.RemoveGroupPhotoAsync(group.Id);
                await photoRepository.RemovePhotoAsync(photoId);
            }
        
            // add new photo
            await photoRepository.AddPhotoAsync(photo);
            await photoRepository.SetGroupPhotoAsync(group.Id, photo.Id);
        
            // commit all changes
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        
            // only send notification if transaction succeeds
            var updatedGroup = await groupRepository.GetGroupWithPhoto(request.GroupId);
            var groupDto = mapper.Map<GroupBasicInfoDTO>(updatedGroup);
            var membersToMap = await groupRepository.GetUsersInGroup(groupDto.Id);
            var members = mapper.Map<List<UserBasicInfo>>(membersToMap);
            await notificationsService.GroupUpdated(groupDto, members, request.OwnerId);
        }
        catch (Exception)
        {
            // rollback on any exception
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}