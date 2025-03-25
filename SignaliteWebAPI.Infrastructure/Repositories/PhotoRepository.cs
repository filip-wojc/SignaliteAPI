using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Infrastructure.Repositories;

// Repository used only to add and remove photos from db, 
// The photos have PublicId's and Url's that lead to a cloudinary cloud media storage
public class PhotoRepository(SignaliteDbContext dbContext, IMediaService mediaService) : IPhotoRepository
{
    // generic function to add any type of photo, other methods handle what photo it will be assigned as
    public async Task AddPhotoAsync(Photo photo)
    {
        await dbContext.Photos.AddAsync(photo);
        await dbContext.SaveChangesAsync();
    }
    
    // generic function to delete any type of photo, it's up to another function to clear the reference
    public async Task RemovePhotoAsync(int photoId)
    {
        var photo = await dbContext.Photos.FindAsync(photoId);
        if (photo != null)
        {
            dbContext.Photos.Remove(photo);
            await dbContext.SaveChangesAsync();
        }
    }
    
    // set the reference to the profile picture in the db
    public async Task SetUserProfilePhotoAsync(int userId, int photoId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.ProfilePhotoId = photoId;
            await dbContext.SaveChangesAsync();
        }
    }
    
    // deletes the reference to the profile photo
    public async Task RemoveUserProfilePhotoAsync(int userId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.ProfilePhotoId = null;
            await dbContext.SaveChangesAsync();
        }
    }

    // sets the reference to a photo that will serve as a background for user
    public async Task SetUserBackgroundPhotoAsync(int userId, int photoId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.BackgroundPhotoId = photoId;
            await dbContext.SaveChangesAsync();
        }
    }

    // removes the reference to a background photo in the db
    public async Task RemoveUserBackgroundPhotoAsync(int userId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.BackgroundPhotoId = null;
            await dbContext.SaveChangesAsync();
        }
    }

    // getter for a profile photo
    public Task<Photo> GetProfilePhotoAsync(int userId)
    {
        throw new NotImplementedException();
    }

    // getter for a background photo
    public Task<Photo> GetBackgroundPhotoAsync(int userId)
    {
        throw new NotImplementedException();
    }
    
    // get both photos because why not, might be useful in the future
    public Task<List<Photo>> GetUserPhotosAsync(int userId)
    {
        throw new NotImplementedException();
    }
    
    public async Task SetGroupPhotoAsync(int groupId, int photoId)
    {
        var group = await dbContext.Groups.FindAsync(groupId);
        if (group != null)
        {
            group.PhotoId = photoId;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveGroupPhotoAsync(int groupId)
    {
        var group = await dbContext.Groups.FindAsync(groupId);
        if (group != null)
        {
            group.PhotoId = null;
            await dbContext.SaveChangesAsync();
        }
    }
}