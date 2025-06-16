using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IGroupRepository
{
    Task CreateGroup(Group group);
    Task ModifyGroupName(string groupName, Group group);
    Task AddUserToGroup(UserGroup userGroup);
    Task DeleteUserFromGroup(Group group, int userId);
    void DeleteGroup(Group group);
    Task<Group> GetGroup(int groupId);
    Task<List<Group>> GetUserGroupsWithPhoto(int userId);
    Task<Group> GetGroupWithPhoto(int groupId);
    Task<Group> GetGroupWithUsers(int groupId);
    Task<List<User>> GetUsersInGroup(int groupId);
    Task<Group> GetGroupMembers(int groupId);
    Task<bool> GroupExists(int groupId);
}