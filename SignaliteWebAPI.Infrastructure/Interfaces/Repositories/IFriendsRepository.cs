using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IFriendsRepository
{
    Task<bool> FriendRequestExists(int senderId, int recipientId);
    Task SendFriendRequest(FriendRequest friendRequest);
    Task<List<FriendRequest>> GetFriendRequests(int userId);
    Task DeleteFriendRequest(FriendRequest friendRequest);
    Task<List<User>> GetUserFriends(int userId);
    Task<List<UserFriend>> GetAllUserFriends();
    Task AddFriend(UserFriend userFriend);
}