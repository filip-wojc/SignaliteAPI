using MediatR;

namespace SignaliteWebAPI.Application.Features.Messages.DeleteMessage;

public class DeleteMessageCommand : IRequest
{
    public int MessageId { get; set; }
    public int SenderId { get; set; }
}