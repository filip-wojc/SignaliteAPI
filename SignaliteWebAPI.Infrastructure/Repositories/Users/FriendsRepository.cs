using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;

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

    public async Task<bool> FriendRequestExists(int senderId, int recipientId)
    {
        var friendRequests = await dbContext.FriendRequests
            .Where(fr =>
                (fr.RecipientId == recipientId && fr.SenderId == senderId) ||
                (fr.RecipientId == senderId && fr.SenderId == recipientId)).ToListAsync();
        return friendRequests.Any();
    }
}