using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface INotificationsService
{
    Task FriendRequest(int recipientUserId, int senderUserId, string senderUsername);
    Task MessageReceived(List<UserBasicInfo> usersInGroup, MessageDTO messageDto);
    Task FriendRequestAccepted(int recipientUserId, int senderUserId, string senderUsername);
    Task AddedToGroup(int recipientUserId, int senderUserId, GroupBasicInfoDTO groupInfoDto);
    Task UserAddedToGroup(UserBasicInfo addedUserInfo, List<UserBasicInfo> usersInGroup);
    Task GroupUpdated(GroupBasicInfoDTO groupDto, List<UserBasicInfo> usersInGroup, int ownerId);
    Task UserRemovedFromGroup(int userId, int groupId, List<UserBasicInfo> usersInGroup);
}