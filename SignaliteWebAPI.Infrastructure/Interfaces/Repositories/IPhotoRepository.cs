using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IPhotoRepository
{
    Task AddPhotoAsync(Photo photo);
    Task RemovePhotoAsync(int photoId);
    Task SetUserProfilePhotoAsync(int userId, int photoId);
    Task RemoveUserProfilePhotoAsync(int userId);
    Task SetUserBackgroundPhotoAsync(int userId, int photoId);
    Task RemoveUserBackgroundPhotoAsync(int userId);
    Task<Photo> GetProfilePhotoAsync(int photoId);
    Task<Photo> GetBackgroundPhotoAsync(int userId);
    Task<List<Photo>> GetUserPhotosAsync(int userId);
}