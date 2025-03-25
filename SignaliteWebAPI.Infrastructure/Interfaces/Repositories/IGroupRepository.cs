using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IGroupRepository
{
    Task CreateGroup(Group group);
    Task AddUserToGroup(UserGroup userGroup);
    Task DeleteUserFromGroup(UserGroup userGroup);
    Task DeleteGroup(Group group);
    Task<Group> GetGroupWithPhoto(int groupId);
    Task<Group> GetGroupDetails(int groupId);
}