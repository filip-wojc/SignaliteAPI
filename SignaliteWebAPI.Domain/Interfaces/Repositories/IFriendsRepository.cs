using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.Interfaces.Repositories;

public interface IFriendsRepository
{
    Task<bool> FriendRequestExists(int senderId, int recipientId);
    Task SendFriendRequest(FriendRequest friendRequest);
    Task<List<FriendRequest>> GetFriendRequests(int userId);
}