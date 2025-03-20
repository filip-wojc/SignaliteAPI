using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Exceptions;

namespace SignaliteWebAPI.Infrastructure.Repositories.Users;

public class FriendsRepository(SignaliteDbContext dbContext) : IFriendsRepository
{
    public async Task SendFriendRequest(FriendRequest friendRequest)
    {
        await dbContext.FriendRequests.AddAsync(friendRequest);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<FriendRequest>> GetFriendRequests(int userId)
    {
         return await dbContext.FriendRequests.Include(fr => fr.Sender).Where(fr => fr.RecipientId == userId).ToListAsync();
    }

    public async Task<FriendRequest> GetFriendRequest(int friendRequestId)
    {
        var friendRequest = await dbContext.FriendRequests.FirstOrDefaultAsync(fr => fr.Id == friendRequestId);
        if (friendRequest == null)
        {
            throw new NotFoundException("Friend request not found");
        }
        return friendRequest;
    }

    public async Task DeleteFriendRequest(FriendRequest friendRequest)
    {
        dbContext.FriendRequests.Remove(friendRequest);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<UserFriend>> GetUserFriends(int userId)
    {
        var user = await dbContext.Users.Include(u => u.Friends).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        return user.Friends;
    }

    public async Task<List<UserFriend>> GetAllUserFriends()
    {
        return await dbContext.UserFriends.ToListAsync();
    }
    
    public async Task AddFriend(UserFriend userFriend)
    {
        await dbContext.UserFriends.AddAsync(userFriend);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> FriendRequestExists(int senderId, int recipientId)
    {
        var friendRequests = await dbContext.FriendRequests
            .Where(fr =>
                (fr.RecipientId == recipientId && fr.SenderId == senderId) ||
                (fr.RecipientId == senderId && fr.SenderId == recipientId)).ToListAsync();
        return friendRequests.Any();
    }
    
}