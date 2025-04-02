using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddMessage(Message message);
    Task<List<Message>> GetMessages(int groupId);
    Task<Message> GetMessageWithAttachment(int messageId);
    Task<Message> GetMessage(int messageId);
    Task DeleteMessage(Message message);
    Task ModifyMessage(string messageContent, Message messageToModify);
}