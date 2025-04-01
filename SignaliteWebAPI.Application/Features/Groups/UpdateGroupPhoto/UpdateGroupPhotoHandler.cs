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
        // TODO: Private conversation photo group return
        var group = await groupRepository.GetGroupWithPhoto(request.GroupId);
        if (group.OwnerId != request.OwnerId)
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
        
        if (group.Photo != null)
        {
            var photoId = group.Photo.Id;

            try
            {
                unitOfWork.BeginTransactionAsync();
                await mediaService.DeleteMediaAsync(group.Photo.PublicId);
                await photoRepository.RemoveGroupPhotoAsync(group.Id);
                await photoRepository.RemovePhotoAsync(photoId);
                unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        try
        {
            unitOfWork.BeginTransactionAsync();
            await photoRepository.AddPhotoAsync(photo);
            await photoRepository.SetGroupPhotoAsync(group.Id, photo.Id);
            unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            unitOfWork.RollbackTransactionAsync();
            throw;
        }
        
        var updatedGroup = await groupRepository.GetGroupWithPhoto(request.GroupId);
        var groupDto = mapper.Map<GroupBasicInfoDTO>(updatedGroup);
        var membersToMap = await groupRepository.GetUsersInGroup(groupDto.Id);
        var members = mapper.Map<List<UserBasicInfo>>(membersToMap);
        await notificationsService.GroupUpdated(groupDto, members, request.OwnerId);
    }
}