using MediatR;

using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.DeleteBackgroundPhoto;

public class DeleteBackgroundPhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService) : IRequestHandler<DeleteBackgroundPhotoCommand, bool>
{
    public async Task<bool> Handle(DeleteBackgroundPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithBackgroundPhotoAsync(request.UserId);
        if (user?.BackgroundPhoto == null) return false; // TODO: throw exception or something idk

        // delete photo from cloud storage
        var deletionResult = await mediaService.DeletePhotoAsync(user.BackgroundPhoto.PublicId);
        if (deletionResult.Error != null) return false; // TODO: also maybe throw exception

        // remove photo from database
        await photoRepository.RemovePhotoAsync(user.BackgroundPhoto.Id);
        
        // update user to remove reference to photo
        await userRepository.RemoveBackgroundPhotoReferenceAsync(user.Id);
        
        return true;
    }
}