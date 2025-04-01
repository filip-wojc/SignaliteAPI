using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Messages.GetMessageThread;

public class GetMessageThreadHandler(
    IMessageRepository messageRepository, 
    IMapper mapper
    ): IRequestHandler<GetMessageThreadQuery, List<MessageDTO>>
{
    public async Task<List<MessageDTO>> Handle(GetMessageThreadQuery request, CancellationToken cancellationToken)
    {
        var messages = await messageRepository.GetMessages(request.GroupId);
        return mapper.Map<List<MessageDTO>>(messages);
    }
}