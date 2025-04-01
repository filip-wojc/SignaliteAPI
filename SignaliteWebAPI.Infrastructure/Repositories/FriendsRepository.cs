using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Infrastructure.Repositories;

public class FriendsRepository(SignaliteDbContext dbContext) : IFriendsRepository
{
    public async Task SendFriendRequest(FriendRequest friendRequest)
    {
        await dbContext.FriendRequests.AddAsync(friendRequest);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<FriendRequest>> GetFriendRequests(int userId)
    {
        return await dbContext.FriendRequests.Include(fr => fr.Sender).Where(fr => fr.RecipientId == userId)
            .ToListAsync();
    }

    public void DeleteFriendRequest(FriendRequest friendRequest)
    {
        dbContext.FriendRequests.Remove(friendRequest);
        // await dbContext.SaveChangesAsync(); Unit of work
    }

    public async Task<List<User>> GetUserFriends(int userId)
    {
        var userFriends = await dbContext.UserFriends.Include(uf => uf.User).ThenInclude(u => u.ProfilePhoto)
            .Include(uf => uf.Friend).ThenInclude(u => u.ProfilePhoto)
            .Where(uf => uf.UserId == userId || uf.FriendId == userId).ToListAsync();
        var friends = userFriends.Select(u => userId == u.UserId ? u.Friend : u.User).ToList();
        return friends;
    }

    public async Task<List<UserFriend>> GetAllUserFriends()
    {
        return await dbContext.UserFriends.ToListAsync();
    }

    public async Task<UserFriend> GetUserFriend(int userId, int friendId)
    {
        var userFriend = await dbContext.UserFriends.FirstOrDefaultAsync(fr =>
            (fr.UserId == userId && fr.FriendId == friendId) || (fr.UserId == friendId && fr.FriendId == userId));
        if (userFriend == null)
        {
            throw new NotFoundException("UserFriend not found");
        }
        return userFriend;
    }

    public async Task AddFriend(UserFriend userFriend)
    {
        await dbContext.UserFriends.AddAsync(userFriend);
        // await dbContext.SaveChangesAsync(); Unit of work
    }

    public void DeleteFriend(UserFriend userFriend)
    {
        dbContext.UserFriends.Remove(userFriend);
        // await dbContext.SaveChangesAsync(); Unit of work
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