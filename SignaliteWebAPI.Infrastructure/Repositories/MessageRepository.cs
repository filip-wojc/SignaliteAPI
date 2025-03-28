using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Infrastructure.Repositories;

public class MessageRepository(SignaliteDbContext dbContext) : IMessageRepository
{
    public async Task AddMessage(Message message)
    {
        await dbContext.Messages.AddAsync(message);
        //await dbContext.SaveChangesAsync(); // UnitOfWork handles saving now
    }

    public async Task<List<Message>> GetMessages(int groupId)
    {
        var messages = await dbContext.Messages.Include(m => m.Sender).ThenInclude(u => u.ProfilePhoto)
            .Include(m => m.Attachment).Where(m => m.GroupId == groupId).ToListAsync();
        return messages;
    }
}