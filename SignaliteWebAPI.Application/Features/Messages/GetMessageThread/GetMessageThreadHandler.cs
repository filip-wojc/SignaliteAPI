using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SignaliteWebAPI.Application.Helpers;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Messages.GetMessageThread;

public class GetMessageThreadHandler(
    IMessageRepository messageRepository,
    IMapper mapper
) : IRequestHandler<GetMessageThreadQuery, PageResult<MessageDTO>>
{
    public async Task<PageResult<MessageDTO>> Handle(GetMessageThreadQuery request, CancellationToken cancellationToken)
    {
        var messagesQueryable = messageRepository.GetMessagesQueryable(request.GroupId);

        int totalItems = messagesQueryable.Count();
        int pageNumber = request.PaginationQuery.PageNumber ?? 1;
        int pageSize = request.PaginationQuery.PageSize ?? 10;

        messagesQueryable = messagesQueryable.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        var messageDtos = mapper.Map<List<MessageDTO>>(await messagesQueryable.ToListAsync());
        return new PageResult<MessageDTO>(messageDtos, totalItems, pageSize, pageNumber);
    }
}