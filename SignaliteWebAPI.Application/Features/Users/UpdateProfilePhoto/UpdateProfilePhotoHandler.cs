// UploadUserPhotoHandler.cs

using MediatR;
using SignaliteWebAPI.Application.Features.Users.AddProfilePhoto;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Interfaces;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Photos;

public class UpdateUserPhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService) : IRequestHandler<UpdateProfilePhotoCommand, bool>
{
    public async Task<bool> Handle(UpdateProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);
        if (user == null) return false; // TODO: Throw something here

        // Upload new photo
        var uploadResult = await mediaService.AddPhotoAsync(request.PhotoFile);
        if (uploadResult.Error != null) return false; // TODO: same here

        // Create new photo entity
        var photo = new Photo
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            UserId = user.Id
        };
        
        // remove old photo if exists
        if (user.ProfilePhoto != null)
        {
            await mediaService.DeletePhotoAsync(user.ProfilePhoto.PublicId);
            await photoRepository.RemoveUserProfilePhotoAsync(user.ProfilePhoto.Id);
        }
        
        // save new photo
        await photoRepository.AddPhotoAsync(photo);
        await photoRepository.SetUserProfilePhotoAsync(user.Id, photo.Id);
        
        return true;
    }
}