using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Database;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Infrastructure.Repositories;

public class AttachmentRepository(SignaliteDbContext dbContext) : IAttachmentRepository
{
    public async Task AddAttachment(Attachment attachment)
    {
        await dbContext.Attachments.AddAsync(attachment);
        //await dbContext.SaveChangesAsync(); // UnitOfWork handles saving now
    }

    public async Task DeleteAttachment(Attachment attachment)
    {
        dbContext.Attachments.Remove(attachment);
        await dbContext.SaveChangesAsync();
    }
}