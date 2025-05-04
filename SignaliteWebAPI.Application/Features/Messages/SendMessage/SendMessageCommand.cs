using MediatR;
using Microsoft.AspNetCore.Http;
using SignaliteWebAPI.Application.Helpers;

namespace SignaliteWebAPI.Application.Features.Messages.SendMessage;

public class SendMessageCommand : IRequest<SendMessageResult>
{
    public SendMessageDTO SendMessageDto  { get; set; }
    public int SenderId { get; set; }
}
