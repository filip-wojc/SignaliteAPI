using MediatR;

namespace SignaliteWebAPI.Application.Features.Messages.ModifyMessage;

public class ModifyMessageCommand : IRequest
{
    public int SenderId { get; set; }
    public string MessageContent { get; set; }
    public int MessageId { get; set; }
}