using MediatR;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.DeleteProfilePhoto;

public class DeleteProfilePhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IMediaService mediaService) : IRequestHandler<DeleteProfilePhotoCommand, bool>
{
    public async Task<bool> Handle(DeleteProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);
        if (user?.ProfilePhoto == null) return false; // TODO: throw exception later or something idk

        // delete photo from cloud storage
        var deletionResult = await mediaService.DeletePhotoAsync(user.ProfilePhoto.PublicId);
        if (deletionResult.Error != null) return false; // TODO: also maybe throw exception

        // remove photo from database
        await photoRepository.RemovePhotoAsync(user.ProfilePhoto.Id);
        
        return true;
    }
}