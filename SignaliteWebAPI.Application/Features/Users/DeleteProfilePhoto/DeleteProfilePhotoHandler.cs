using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.DeleteProfilePhoto;

public class DeleteProfilePhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IFriendsRepository friendsRepository,
    IMediaService mediaService,
    INotificationsService notificationsService,
    IMapper mapper
    ): IRequestHandler<DeleteProfilePhotoCommand>
{
    public async Task Handle(DeleteProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserWithProfilePhotoAsync(request.UserId);
        if (user?.ProfilePhoto == null) 
            throw new NotFoundException("Profile photo is missing");

        // delete photo from cloud storage
        var deletionResult = await mediaService.DeleteMediaAsync(user.ProfilePhoto.PublicId);
        if (deletionResult.Error != null) 
            throw new CloudinaryException(deletionResult.Error.Message);

        // remove photo from database
        await photoRepository.RemovePhotoAsync(user.ProfilePhoto.Id);
        
        var friendsToMap = await friendsRepository.GetUserFriends(request.UserId);
        var usersToNotify = mapper.Map<List<UserBasicInfo>>(friendsToMap);
        await notificationsService.UserUpdated(user.Id, usersToNotify);
        // TODO: test
    }
    
}