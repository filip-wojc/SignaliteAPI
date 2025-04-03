using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Helpers;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Infrastructure.Repositories;

public class MessageRepository(SignaliteDbContext dbContext) : IMessageRepository
{
    public async Task AddMessage(Message message)
    {
        await dbContext.Messages.AddAsync(message);
        //await dbContext.SaveChangesAsync(); // UnitOfWork handles saving now
    }

    public IQueryable<Message> GetMessagesQueryable(int groupId)
    {
        // Sortuj wszystko od najnowszych aby kolejne strony w paginacji miały starsze wiadomości
        var messagesQuery = dbContext.Messages.OrderByDescending(m => m.DateSent).Include(m => m.Sender)
            .ThenInclude(u => u.ProfilePhoto)
            .Include(m => m.Attachment).Where(m => m.GroupId == groupId).AsQueryable();

        return messagesQuery;
    }

    public async Task<List<Message>> GetMessages(int groupId)
    {
        var messages = await dbContext.Messages.Include(m => m.Sender).ThenInclude(u => u.ProfilePhoto)
            .Include(m => m.Attachment).Where(m => m.GroupId == groupId).ToListAsync();
        return messages;
    }

    public async Task<Message> GetMessageWithAttachment(int messageId)
    {
        var message = await dbContext.Messages.Include(m => m.Attachment).FirstOrDefaultAsync(m => m.Id == messageId);
        if (message == null)
        {
            throw new NotFoundException("Message not found");
        }

        return message;
    }

    public async Task<Message> GetMessage(int messageId)
    {
        var message = await dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message == null)
        {
            throw new NotFoundException("Message not found");
        }

        return message;
    }

    public async Task DeleteMessage(Message message)
    {
        dbContext.Messages.Remove(message);
        await dbContext.SaveChangesAsync();
    }

    public async Task ModifyMessage(string messageContent, Message messageToModify)
    {
        messageToModify.Content = messageContent;
        messageToModify.DateModified = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
}