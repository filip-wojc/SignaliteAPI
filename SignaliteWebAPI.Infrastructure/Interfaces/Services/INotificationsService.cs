using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface INotificationsService
{
    Task FriendRequest(FriendRequestDTO friendRequest, int recipientId);
    Task FriendRequestAccepted(UserDTO userWhoAccepted, int senderId);
    Task MessageReceived(List<UserBasicInfo> usersInGroup, MessageDTO messageDto);
    Task MessageModified(MessageDTO messageDto, int groupId, List<UserBasicInfo> usersInGroup);
    Task MessageDeleted(int groupId, int messageId,int senderId ,List<UserBasicInfo> usersInGroup);
    Task AttachmentRemoved(int groupId, int messageId, int senderId, List<UserBasicInfo> usersInGroup);
    Task AddedToGroup(int recipientUserId, int senderUserId, GroupBasicInfoDTO groupInfoDto);
    Task UserAddedToGroup(UserBasicInfo addedUserInfo, List<UserBasicInfo> usersInGroup);
    Task GroupUpdated(GroupBasicInfoDTO groupDto, List<UserBasicInfo> usersInGroup, int ownerId);
    Task UserRemovedFromGroup(int userId, int groupId, List<UserBasicInfo> usersInGroup);
    Task GroupDeleted(int groupId, int ownerId,List<UserBasicInfo> usersInGroup);
    Task UserUpdated(int userId, List<UserBasicInfo> userFriends);
}