using MediatR;
using SignaliteWebAPI.Application.Helpers;
using SignaliteWebAPI.Domain.DTOs.Messages;

namespace SignaliteWebAPI.Application.Features.Messages.GetMessageThread;

public class GetMessageThreadQuery : IRequest<PageResult<MessageDTO>>
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public PaginationQuery PaginationQuery { get; set; }
}