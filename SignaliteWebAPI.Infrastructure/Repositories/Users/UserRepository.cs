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

    public async Task<User?> GetUserById(int userId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
    
}