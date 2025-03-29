using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;
using SignaliteWebAPI.Infrastructure.SignalR;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Infrastructure.Services;

public class NotificationsService(
    PresenceTracker presenceTracker,
    IHubContext<NotificationsHub> notificationsHub, 
    ILogger logger
    ):INotificationsService
{
    public async Task SendFriendRequestNotification(int recipientUserId, int senderUserId, string senderUsername) 
    {
        try
        {
            // Get all online users detailed info (id + username)
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            
            // Find the recipient user
            var recipientUser = onlineUsers.FirstOrDefault(u => u.UserId == recipientUserId);
            if (recipientUser == null)
            {
                logger.Debug($"Cannot send friend request notification - recipient user (ID: {recipientUserId}) is not online");
                return;
            }
            
            // Create notification object
            var notification = new
            {
                SenderId = senderUserId,
                SenderUsername = senderUsername,
                Timestamp = DateTime.UtcNow
            };
            
            // Send notification to the user using their username as the identifier
            // This sends to ALL connections this user has to the NotificationsHub
            await notificationsHub.Clients
                .User(recipientUser.Username)
                .SendAsync("FriendRequest", notification);
            
            logger.Information($"Friend request notification sent from {senderUsername} (ID: {senderUserId}) to {recipientUser.Username} (ID: {recipientUserId})");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error sending friend request notification from user ID {senderUserId} to user ID {recipientUserId}");
        }
    }
    
    public async Task SendFriendRequestAcceptedNotification(int recipientUserId, int senderUserId, string senderUsername)
    {
        try
        {
            // Get all online users detailed info (id + username)
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            
            var recipientUser = onlineUsers.FirstOrDefault(u => u.UserId == recipientUserId);
            if (recipientUser == null)
            {
                logger.Debug($"Cannot send friend request accepted notification - recipient user (ID: {recipientUserId}) is not online");
                return;
            }
            
            // Create notification object
            var notification = new
            {
                SenderId = senderUserId,
                SenderUsername = senderUsername,
                Timestamp = DateTime.UtcNow
            };
            
            // Send notification to the user using their username as the identifier
            await notificationsHub.Clients
                .User(recipientUser.Username)
                .SendAsync("FriendRequestAccepted", notification);
            
            logger.Information($"Friend request accepted notification sent from {senderUsername} (ID: {senderUserId}) to {recipientUser.Username} (ID: {recipientUserId})");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error sending friend request accepted notification from user ID {senderUserId} to user ID {recipientUserId}");
        }
    }
}