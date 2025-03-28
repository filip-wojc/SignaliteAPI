using MediatR;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.DeleteBackgroundPhoto;

public class DeleteBackgroundPhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService) : IRequestHandler<DeleteBackgroundPhotoCommand>
{
    public async Task Handle(DeleteBackgroundPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithBackgroundPhotoAsync(request.UserId);
        if (user?.BackgroundPhoto == null)
            throw new NotFoundException("Background photo is missing");// TODO: throw exception or something idk

        // delete photo from cloud storage
        var deletionResult = await mediaService.DeleteMediaAsync(user.BackgroundPhoto.PublicId, "image/jpeg");
        if (deletionResult.Error != null) 
            throw new CloudinaryException(deletionResult.Error.Message);

        // remove photo from database
        await photoRepository.RemovePhotoAsync(user.BackgroundPhoto.Id);
        
    }
}