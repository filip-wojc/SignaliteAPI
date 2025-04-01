using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;
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
    public async Task FriendRequest(int recipientUserId, int senderUserId, string senderUsername) 
    {
        try
        {
            // Get all online users detailed info (id + username)
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            
            // Find the recipient user
            var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientUserId);
            if (recipientUser == null)
            {
                logger.Debug($"[NotificationsService] Cannot send FriendRequest notification - recipient user (ID: {recipientUserId}) is not online");
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
            
            logger.Debug($"[NotificationsService] FriendRequest notification sent from {senderUsername} (ID: {senderUserId}) to {recipientUser.Username} (ID: {recipientUserId})");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"[NotificationsService] Error sending FriendRequest notification from user ID {senderUserId} to user ID {recipientUserId}");
        }
    }
    
    public async Task FriendRequestAccepted(int recipientUserId, int senderUserId, string senderUsername)
    {
        try
        {
            // Get all online users detailed info (id + username)
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            
            var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientUserId);
            if (recipientUser == null)
            {
                logger.Debug($"[NotificationsService] Cannot send FriendRequestAccepted notification - recipient user (ID: {recipientUserId}) is not online");
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
            
            logger.Debug($"F[NotificationsService] FriendRequestAccepted notification sent from {senderUsername} (ID: {senderUserId}) to {recipientUser.Username} (ID: {recipientUserId})");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"[NotificationsService] Error sending FriendRequestAccepted notification from user ID {senderUserId} to user ID {recipientUserId}");
        }
    }

    public async Task MessageReceived(List<UserBasicInfo> usersInGroup, MessageDTO messageDto)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds(); // get online users from tracker
        
        // get the intersection of online/group users (without the sender)
        var onlineGroupUsers = usersInGroup
            .Where(user => onlineUsers.Contains(user.Id) && user.Id != messageDto.Sender.Id)
            .ToList();
        
        // if no users online, don't send a notification
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send MessageReceived notification for message ID: {messageDto.Id}");
            return;
        }
        
        // send notification with messageDto to online group users
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("MessageReceived", messageDto);
            logger.Debug($"[NotificationsService] MessageReceived received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] MessageReceived notification sent to {onlineGroupUsers.Count} online users in group for message ID: {messageDto.Id}");
    }

    public async Task AddedToGroup(int recipientUserId, int senderUserId, GroupBasicInfoDTO groupInfoDto)
    {
        var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
        
        // check if user is online
        var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientUserId);
        if (recipientUser == null)
        {
            logger.Debug($"[NotificationsService] Cannot send AddedToGroup notification - recipient user (ID: {recipientUserId}) is not online");
            return;
        }
        
        await notificationsHub.Clients
            .User(recipientUser.Username)
            .SendAsync("AddedToGroup", groupInfoDto);
        
        logger.Debug($"[NotificationsService] AddedToGroup notification sent to {recipientUser.Username} (ID: {recipientUser.Id}) from  (ID: {senderUserId})");
    }

    public async Task UserAddedToGroup(UserBasicInfo addedUserInfo,  List<UserBasicInfo> usersInGroup)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineGroupUsers = usersInGroup
            .Where(user => onlineUsers.Contains(user.Id) && user.Id != addedUserInfo.Id)
            .ToList();
        
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send UserAddedToGroupNotification for user ID: {addedUserInfo.Id}");
            return;
        }
        
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("UserAddedToGroup", addedUserInfo);
            logger.Debug($"[NotificationsService] UserAddedToGroup received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] UserAddedToGroup notification sent to {onlineGroupUsers.Count} online users in group for user ID: {addedUserInfo.Id}");
    }

    public async Task GroupUpdated(GroupBasicInfoDTO groupDto, List<UserBasicInfo> usersInGroup, int ownerId)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineGroupUsers = usersInGroup
                .Where(user => onlineUsers.Contains(user.Id) && user.Id != ownerId)
                .ToList();
        
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send GroupUpdated notification for group ID: {groupDto.Id}");
            return;
        }
        
        // need to create a dto because passing group will make JsonSerializer angry

        
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("GroupUpdated", groupDto);
            logger.Debug($"[NotificationsService] GroupUpdated received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] GroupUpdated notification sent to {onlineGroupUsers.Count} online users in group for group ID: {groupDto.Id}");
        
    }

    public async Task UserRemovedFromGroup(int userId, int groupId ,List<UserBasicInfo> usersInGroup)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineGroupUsers = usersInGroup
            .Where(user => onlineUsers.Contains(user.Id))
            .ToList();
        
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send UserRemovedFromGroup notification for group ID: {groupId}");
            return;
        }

        var notification = new
        {
            UserId = userId,
            GroupId = groupId,
        };
        
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("UserRemovedFromGroup", notification);
            logger.Debug($"[NotificationsService] UserRemovedFromGroup received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] UserRemovedFromGroup notification sent to {onlineGroupUsers.Count} online users in group for group ID: {groupId}");
    }
}