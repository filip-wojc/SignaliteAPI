using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddUser(User user);
    Task<User> GetUserByEmail(string email);
    Task<User?> GetUserById(int userId);
    Task UpdateRefreshToken(int userId, string refreshToken, DateTime expiry);
    Task<User> GetUserByRefreshToken(string refreshToken);
}