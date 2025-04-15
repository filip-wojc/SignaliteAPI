using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
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
    public async Task FriendRequest(FriendRequestDTO friendRequest, int recipientId) 
    {
        try
        {
            // Get all online users detailed info (id + username)
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            
            // Find the recipient user
            var recipientUser = onlineUsers.FirstOrDefault(u => u.Id == recipientId);
            if (recipientUser == null)
            {
                logger.Debug($"[NotificationsService] Cannot send FriendRequest notification - recipient user (ID: {recipientId}) is not online");
                return;
            }
            
            // Create notification object
            var notification = friendRequest;
            
            // Send notification to the user using their username as the identifier
            // This sends to ALL connections this user has to the NotificationsHub
            await notificationsHub.Clients
                .User(recipientUser.Username)
                .SendAsync("FriendRequest", notification);
            
            logger.Debug($"[NotificationsService] FriendRequest notification sent from {friendRequest.SenderId} (ID: {friendRequest.SenderId}) to {recipientUser.Username} (ID: {recipientId})");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"[NotificationsService] Error sending FriendRequest notification from user ID {friendRequest.SenderId} to user ID {recipientId}");
        }
    }
    
    public async Task FriendRequestAccepted(UserBasicInfo userWhoAccepted, int senderId)
    {
        try
        {
            // Get all online users detailed info (id + username)
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            
            var sender = onlineUsers.FirstOrDefault(u => u.Id == senderId);
            if (sender == null)
            {
                logger.Debug($"[NotificationsService] Cannot send FriendRequestAccepted notification - recipient user (ID: {senderId}) is not online");
                return;
            }
            
            // Create notification object
            var notification = userWhoAccepted;
            
            // Send notification to the user using their username as the identifier
            await notificationsHub.Clients
                .User(sender.Username)
                .SendAsync("FriendRequestAccepted", notification);
            
            logger.Debug($"F[NotificationsService] FriendRequestAccepted notification sent from {userWhoAccepted.Username} (ID: {userWhoAccepted.Id}) to {sender.Username} (ID: {senderId})");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"[NotificationsService] Error sending FriendRequestAccepted notification from user ID {userWhoAccepted.Id} to user ID {senderId}");
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

    public async Task MessageModified(MessageDTO messageDto, int groupId, List<UserBasicInfo> usersInGroup)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineGroupUsers = usersInGroup.Where(user => onlineUsers.Contains(user.Id) && user.Id != messageDto.Sender.Id).ToList();
        
        // if no users online, don't send a notification
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send MessageModified notification for message ID: {messageDto.Id}");
            return;
        }
        
        var notification = new
        {
            GroupId = groupId,
            Message = messageDto
        };
        
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("MessageModified", notification);
            logger.Debug($"[NotificationsService] MessageModified received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] MessageModified notification sent to {onlineGroupUsers.Count} online users in group for message ID: {messageDto.Id}");
    }
    
    public async Task MessageDeleted(int groupId, int messageId,int senderId, List<UserBasicInfo> usersInGroup)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineGroupUsers = usersInGroup
            .Where(user => onlineUsers.Contains(user.Id) && user.Id != senderId)
            .ToList();
        
        // if no users online, don't send a notification
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send MessageDeleted notification for message ID: {messageId}");
            return;
        }

        var notification = new
        {
            GroupId = groupId,
            MessageId = messageId
        };
        
        // send notification with messageDto to online group users
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("MessageDeleted", notification);
            logger.Debug($"[NotificationsService] MessageDeleted received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] MessageDeleted notification sent to {onlineGroupUsers.Count} online users in group for message ID: {messageId}");

    }

    public async Task AttachmentRemoved(int groupId, int messageId, int senderId, List<UserBasicInfo> usersInGroup)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        // get the intersection of online/group users (without the sender)
        var onlineGroupUsers = usersInGroup
            .Where(user => onlineUsers.Contains(user.Id) && user.Id != senderId)
            .ToList();
        
        // if no users online, don't send a notification
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send AttachmentRemoved notification for message ID: {messageId}");
            return;
        }

        var notification = new
        {
            GroupId = groupId,
            MessageId = messageId
        };
        
        // send notification with messageDto to online group users
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("AttachmentRemoved", notification);
            logger.Debug($"[NotificationsService] AttachmentRemoved received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] AttachmentRemoved notification sent to {onlineGroupUsers.Count} online users in group for message ID: {messageId}");
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

    public async Task GroupDeleted(int groupId,int ownerId, List<UserBasicInfo> usersInGroup)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineGroupUsers = usersInGroup.Where(user => onlineUsers.Contains(user.Id) && user.Id != ownerId).ToList();
        
        if (onlineGroupUsers.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send GroupDeleted notification for group ID: {groupId}");
            return;
        }
        
        var notification = new
        {
            GroupId = groupId,
        };
        
        foreach (var user in onlineGroupUsers)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("GroupDeleted", notification);
            logger.Debug($"[NotificationsService] GroupDeleted received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] GroupDeleted notification sent to {onlineGroupUsers.Count} online users in group for group ID: {groupId}");
    }

    public async Task UserUpdated(int userId,List<UserBasicInfo> userFriends)
    {
        var onlineUsers = await presenceTracker.GetOnlineUserIds();
        
        var onlineUserFriends = userFriends.Where(user => onlineUsers.Contains(user.Id)).ToList();
        
        // if no users online, don't send a notification
        if (onlineUserFriends.Count == 0)
        {
            logger.Debug($"[NotificationsService] No online users in group to send UserUpdated notification for user ID: {userId}");
            return;
        }
        
        var notification = new
        {
            UserId = userId,
        };
        
        foreach (var user in onlineUserFriends)
        {
            await notificationsHub.Clients
                .User(user.Username)
                .SendAsync("UserUpdated", notification);
            logger.Debug($"[NotificationsService] UserUpdated received from {user.Username} (ID: {user.Id})");
        }
        
        logger.Debug($"[NotificationsService] UserUpdated notification sent to {onlineUserFriends.Count} online users in group for user ID: {userId}");
    }
    
}