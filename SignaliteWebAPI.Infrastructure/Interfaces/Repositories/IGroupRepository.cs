using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IGroupRepository
{
    Task CreateGroup(Group group);
    Task<Group> GetGroup(int groupId);
    Task AddUserToGroup(UserGroup userGroup);
    Task DeleteUserFromGroup(Group group, int userId);
    Task DeleteGroup(Group group);
    Task<Group> GetGroupWithPhoto(int groupId);
    Task<Group> GetGroupWithUsers(int groupId);
    Task<List<User>> GetUsersInGroup(int groupId);
    Task<Group> GetGroupDetails(int groupId);
    Task<bool> GroupExists(int groupId);
}