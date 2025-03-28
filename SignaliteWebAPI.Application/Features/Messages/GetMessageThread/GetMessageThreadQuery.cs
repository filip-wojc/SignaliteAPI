using MediatR;
using SignaliteWebAPI.Domain.DTOs.Messages;

namespace SignaliteWebAPI.Application.Features.Messages.GetMessageThread;

public class GetMessageThreadQuery : IRequest<List<MessageDTO>>
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
}