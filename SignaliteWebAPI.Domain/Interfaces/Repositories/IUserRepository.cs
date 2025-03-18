using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddUser(User user);
    Task<User?> GetUserById(int userId);
    
    // later we might move these methods into FriendRequestRepository, for now its in UserRepository
    Task<bool> IsFriendRequestExist(int senderId, int recipientId);
    Task SendFriendRequest(FriendRequest friendRequest);
    
}