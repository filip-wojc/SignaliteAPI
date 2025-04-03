using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Helpers;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddMessage(Message message);
    IQueryable<Message> GetMessagesQueryable(int groupId);
    Task<List<Message>> GetMessages(int groupId);
    Task<Message> GetMessageWithAttachment(int messageId);
    Task<Message> GetMessage(int messageId);
    Task DeleteMessage(Message message);
    Task ModifyMessage(string messageContent, Message messageToModify);
}