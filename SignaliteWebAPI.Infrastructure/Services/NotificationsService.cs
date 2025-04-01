using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;
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
            var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientUserId);
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
            
            var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientUserId);
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

    public async Task SendMessageReceivedNotification(List<UserBasicInfo> usersInGroup, MessageDTO messageDto)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds(); // get online users from tracker
        
        // get the intersection of online/group users (without the sender)
        var onlineGroupUsers = usersInGroup
            .Where(groupUser => onlineUsers.Contains(groupUser.Id) && groupUser.Id != messageDto.Sender.Id)
            .ToList();
        
        // if no users online, dont send a notification
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"No online users in group to send message notification for message ID: {messageDto.Id}");
            return;
        }
        
        // send notification with messageDto to online group users
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("MessageReceived", messageDto);
            logger.Debug($"Message received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"Message notification sent to {onlineGroupUsers.Count} online users in group for message ID: {messageDto.Id}");
    }

    public async Task SendAddedToGroupNotification(int recipientUserId, int senderUserId, GroupBasicInfoDTO groupInfoDto)
    {
        var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
        
        // check if user is online
        var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientUserId);
        if (recipientUser == null)
        {
            logger.Debug($"Cannot send friend request accepted notification - recipient user (ID: {recipientUserId}) is not online");
            return;
        }
        
        await notificationsHub.Clients
            .User(recipientUser.Username)
            .SendAsync("AddedToGroup", groupInfoDto);
        
        logger.Debug($"[NotificationsService] AddedToGroup notification sent to {recipientUser.Username} (ID: {recipientUser.Id}) from  (ID: {senderUserId})");
    }
}