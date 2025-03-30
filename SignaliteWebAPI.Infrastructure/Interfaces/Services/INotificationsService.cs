namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface INotificationsService
{
    Task SendFriendRequestNotification(int recipientUserId, int senderUserId, string senderUsername);
    Task SendFriendRequestAcceptedNotification(int recipientUserId, int senderUserId, string senderUsername);
}