using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface INotificationsService
{
    Task SendFriendRequestNotification(int recipientUserId, int senderUserId, string senderUsername);
    Task SendMessageReceivedNotification(List<UserBasicInfo> usersInGroup, MessageDTO messageDto);
    Task SendFriendRequestAcceptedNotification(int recipientUserId, int senderUserId, string senderUsername);
}