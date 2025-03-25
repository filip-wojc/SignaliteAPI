using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;

public class UpdateGroupPhotoHandler(
    IGroupRepository groupRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService) : IRequestHandler<UpdateGroupPhotoCommand>
{
    public async Task Handle(UpdateGroupPhotoCommand request, CancellationToken cancellationToken)
    {
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
            await mediaService.DeletePhotoAsync(group.Photo.PublicId);
            await photoRepository.RemoveGroupPhotoAsync(group.Id);
            await photoRepository.RemovePhotoAsync(photoId);
        }
        
        await photoRepository.AddPhotoAsync(photo);
        await photoRepository.SetGroupPhotoAsync(group.Id, photo.Id);
    }
}