using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Messages.ModifyMessage;

public class ModifyMessageHandler(IMessageRepository messageRepository) : IRequestHandler<ModifyMessageCommand>
{
    public async Task Handle(ModifyMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetMessage(request.MessageId);
        if (message.SenderId != request.SenderId)
        {
            throw new ForbidException("You are not allowed to modify this message.");
        }

        await messageRepository.ModifyMessage(request.MessageContent, message);
    }
    // TODO: Notification
}