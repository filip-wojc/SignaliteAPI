using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Infrastructure.Repositories;

public class UserRepository(SignaliteDbContext dbContext) : IUserRepository
{
    public async Task AddUser(User user)
    {
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task ChangePassword(int userId, string newPassword)
    {
        var user = await dbContext.Users.FindAsync(userId);
        
        if (user == null)
            throw new NotFoundException("User not found");
        
        user.HashedPassword = newPassword;
        
        await dbContext.SaveChangesAsync();
    }

    public async Task ModifyUser(int userId, string username, string email,string  name, string surname)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");
        
        user.Username = username;
        user.Email = email;
        user.Name = name;
        user.Surname = surname;
        
        await dbContext.SaveChangesAsync();
    }
    
    public async Task<User?> GetUserByEmail(string email)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> GetUserByUsername(string username)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public async Task<User> GetUserById(int userId)
    {
        var user = await dbContext.Users.Include(u => u.ProfilePhoto).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        return user;
    }
    
    public async Task UpdateRefreshToken(int userId, string refreshToken, DateTime expiry)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null) return;
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = expiry;
        
        await dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetUserByRefreshToken(string refreshToken)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
    
    public async Task<User?> GetUserWithProfilePhotoAsync(int userId)
    {
        return await dbContext.Users
            .Include(u => u.ProfilePhoto)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<User?> GetUserWithBackgroundPhotoAsync(int userId)
    {
        return await dbContext.Users
            .Include(u => u.BackgroundPhoto)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    

}