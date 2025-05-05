using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddUser(User user);
    Task ChangePassword(User user);
    Task ModifyUser(User user);
    Task<User?> GetUserByEmail(string email);
    Task<User> GetUserByUsername(string username);
    Task<User?> GetUserByUsernameNullable(string username);
    Task<User> GetUserById(int userId);
    Task UpdateRefreshToken(int userId, string refreshToken, DateTime expiry);
    Task<User?> GetUserByRefreshToken(string refreshToken);
    Task<User?> GetUserWithProfilePhotoAsync(int userId);
    Task<User?> GetUserWithBackgroundPhotoAsync(int userId);

}