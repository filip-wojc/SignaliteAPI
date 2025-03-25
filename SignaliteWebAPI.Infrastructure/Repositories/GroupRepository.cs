using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Infrastructure.Repositories;

public class GroupRepository(SignaliteDbContext dbContext) : IGroupRepository
{
    public async Task CreateGroup(Group group)
    {
        await dbContext.Groups.AddAsync(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddUserToGroup(UserGroup userGroup)
    {
        await dbContext.UserGroups.AddAsync(userGroup);
        await dbContext.SaveChangesAsync();
    }

    public Task DeleteUserFromGroup(UserGroup userGroup)
    {
        throw new NotImplementedException();
    }

    public Task DeleteGroup(Group group)
    {
        throw new NotImplementedException();
    }
    
    public async Task<Group> GetGroupWithPhoto(int groupId)
    {
        var group = await dbContext.Groups.Include(g => g.Photo).FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }

        return group;
    }

    public async Task<Group> GetGroupDetails(int groupId)
    {
        var group = await dbContext.Groups.Include(g => g.Photo).Include(g => g.Owner).ThenInclude(o => o.ProfilePhoto)
            .Include(g => g.Users)
            .ThenInclude(u => u.User).ThenInclude(u => u.ProfilePhoto).FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }

        return group;
    }
}