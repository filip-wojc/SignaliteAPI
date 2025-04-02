using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

public interface IAttachmentRepository
{
    Task AddAttachment(Attachment attachment);
    Task DeleteAttachment(Attachment attachment);
}