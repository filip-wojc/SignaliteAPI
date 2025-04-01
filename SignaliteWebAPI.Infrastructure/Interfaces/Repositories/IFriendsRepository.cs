using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IFriendsRepository
{
    Task<bool> FriendRequestExists(int senderId, int recipientId);
    Task SendFriendRequest(FriendRequest friendRequest);
    Task<List<FriendRequest>> GetFriendRequests(int userId);
    void DeleteFriendRequest(FriendRequest friendRequest);
    Task<List<User>> GetUserFriends(int userId);
    Task<List<UserFriend>> GetAllUserFriends();
    Task<UserFriend> GetUserFriend(int userId, int friendId);
    Task AddFriend(UserFriend userFriend);
    void DeleteFriend(UserFriend userFriend);
}