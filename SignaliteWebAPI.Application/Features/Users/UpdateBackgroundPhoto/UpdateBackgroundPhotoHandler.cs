using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;

public class UpdateBackgroundPhotoHandler(
    IUserRepository userRepository,
    IPhotoRepository photoRepository,
    IFriendsRepository friendsRepository,
    IMediaService mediaService,
    INotificationsService notificationsService,
    IMapper mapper
    ): IRequestHandler<UpdateBackgroundPhotoCommand>
{
    public async Task Handle(UpdateBackgroundPhotoCommand request, CancellationToken cancellationToken)
    {
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
            var photoId = user.BackgroundPhoto.Id;
            await mediaService.DeleteMediaAsync(user.BackgroundPhoto.PublicId);
            await photoRepository.RemoveUserBackgroundPhotoAsync(user.Id);
            await photoRepository.RemovePhotoAsync(photoId);
        }
        
        // save new photo
        await photoRepository.AddPhotoAsync(photo);
        await photoRepository.SetUserBackgroundPhotoAsync(user.Id, photo.Id);
        var friendsToMap = await friendsRepository.GetUserFriends(user.Id);
        var usersToNotify = mapper.Map<List<UserBasicInfo>>(friendsToMap);
        
        var userDto = mapper.Map<UserDTO>(user);
        await notificationsService.UserUpdated(userDto, userDto.Username, usersToNotify);
    }
}