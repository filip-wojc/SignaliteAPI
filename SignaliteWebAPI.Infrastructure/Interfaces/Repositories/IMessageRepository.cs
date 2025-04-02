using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddMessage(Message message);
    Task<List<Message>> GetMessages(int groupId);
    Task<Message> GetMessageWithAttachment(int messageId);
    Task DeleteMessage(Message message);
}