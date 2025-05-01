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
        //await dbContext.SaveChangesAsync(); Unit of work 
    }

    public async Task ModifyGroupName(string groupName, Group group)
    {
        group.Name = groupName;
        await dbContext.SaveChangesAsync();
    }

    public async Task AddUserToGroup(UserGroup userGroup)
    {
        await dbContext.UserGroups.AddAsync(userGroup);
        //await dbContext.SaveChangesAsync(); Unit of work
    }

    public async Task DeleteUserFromGroup(Group group, int userId)
    {
        var userToDelete = group.Users.FirstOrDefault(u => u.UserId == userId);
        if (userToDelete == null)
        {
            throw new NotFoundException("User is not a member of this group");
        }

        dbContext.UserGroups.Remove(userToDelete);
        await dbContext.SaveChangesAsync();
    }

    public void DeleteGroup(Group group)
    {
        foreach (var user in group.Users)
        {
            dbContext.UserGroups.Remove(user);
        }

        dbContext.Groups.Remove(group);
    }

    public async Task<Group> GetGroup(int groupId)
    {
        var group = await dbContext.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }

        return group;
    }


    public async Task<List<Group>> GetUserGroupsWithPhoto(int userId)
    {
        return await dbContext.Groups
            // include group photo if present for mapper
            .Include(g => g.Photo)
            // 
            .Include(g => g.Users)
            .ThenInclude(ug => ug.User)
            .ThenInclude(u => u.ProfilePhoto)
            // include messages with sender so mapper can set sender and message to LastMessage in form "Username: message"
            .Include(g => g.Messages)
            .ThenInclude(m => m.Sender)
            // include attachment for mapper to check if attachment present and modify message accordingly in form "Username: sent an attachemtn" instead
            .Include(g => g.Messages)
            .ThenInclude(m => m.Attachment)
            
            .Where(g => g.Users.Any(u => u.UserId == userId))
            .ToListAsync();
    }

    public async Task<Group> GetGroupWithPhoto(int groupId)
    {
        var group = await dbContext.Groups.Include(g => g.Photo).Include(g => g.Messages)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }

        return group;
    }

    public async Task<Group> GetGroupWithUsers(int groupId)
    {
        var group = await dbContext.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }

        return group;
    }

    // get only users in group
    public async Task<List<User>> GetUsersInGroup(int groupId)
    {
        var users = await dbContext.Users.Include(u => u.ProfilePhoto)
            .Where(u => u.Groups.Any(ug => ug.GroupId == groupId))
            .ToListAsync();

        return users;
    }

    public async Task<Group> GetGroupMembers(int groupId)
    {
        var group = await dbContext.Groups.Include(g => g.Owner).ThenInclude(o => o.ProfilePhoto)
            .Include(g => g.Users)
            .ThenInclude(u => u.User).ThenInclude(u => u.ProfilePhoto).FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            throw new NotFoundException("Group not found");
        }

        return group;
    }

    public Task<bool> GroupExists(int groupId)
    {
        return dbContext.Groups.AnyAsync(g => g.Id == groupId);
    }
}