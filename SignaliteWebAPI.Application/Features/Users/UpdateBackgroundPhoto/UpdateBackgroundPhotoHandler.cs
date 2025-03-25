using System.ComponentModel.DataAnnotations;
using MediatR;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;

public class UpdateBackgroundPhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService ) : IRequestHandler<UpdateBackgroundPhotoCommand>
{
    public async Task Handle(UpdateBackgroundPhotoCommand request, CancellationToken cancellationToken)
    {
        if (request.PhotoFile == null)
        {
            // TODO: Change to another exception
            throw new ValidationException("PhotoFile is required");
        }
        
        var user = await userRepository.GetUserWithBackgroundPhotoAsync(request.UserId);
        if (user == null) 
            throw new NotFoundException("User not found");

        // Upload new photo
        var uploadResult = await mediaService.AddPhotoAsync(request.PhotoFile);
        if (uploadResult.Error != null) 
            throw new CloudinaryException(uploadResult.Error.Message);

        // Create new photo entity
        var photo = new Photo
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            UserId = user.Id
        };
        
        // remove old photo if exists
        if (user.BackgroundPhoto != null)
        {
            await mediaService.DeletePhotoAsync(user.BackgroundPhoto.PublicId);
            await photoRepository.RemoveUserBackgroundPhotoAsync(user.BackgroundPhoto.Id);
        }
        
        // save new photo
        await photoRepository.AddPhotoAsync(photo);
        await photoRepository.SetUserBackgroundPhotoAsync(user.Id, photo.Id);
        
    }
}