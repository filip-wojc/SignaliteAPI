using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.Interfaces.Repositories;

public interface IFriendsRepository
{
    Task<bool> FriendRequestExists(int senderId, int recipientId);
    Task<FriendRequest> GetFriendRequest(int friendRequestId);
    Task SendFriendRequest(FriendRequest friendRequest);
    Task<List<FriendRequest>> GetFriendRequests(int userId);
    Task DeleteFriendRequest(FriendRequest friendRequest);
    Task<List<UserFriend>> GetUserFriends(int userId);
    Task<List<UserFriend>> GetAllUserFriends();
    Task DeleteFriend(UserFriend friend);
    Task AddFriend(UserFriend userFriend);
}