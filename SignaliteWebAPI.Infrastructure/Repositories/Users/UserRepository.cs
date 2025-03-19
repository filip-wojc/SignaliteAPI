using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;

namespace SignaliteWebAPI.Infrastructure.Repositories.Users;

public class UserRepository(SignaliteDbContext dbContext) : IUserRepository
{
    public async Task AddUser(User user)
    {
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public async Task<User?> GetUserByEmail(string email)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> GetUserById(int userId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task UpdateRefreshToken(int userId, string refreshToken, DateTime expiry)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null) return;
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = expiry;
        
        await dbContext.SaveChangesAsync();
    }

    public async Task<User> GetUserByRefreshToken(string refreshToken)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
}