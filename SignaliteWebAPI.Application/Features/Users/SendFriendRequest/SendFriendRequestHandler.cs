using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.Features.Users.SendFriendRequest;

public class SendFriendRequestHandler(IFriendsRepository repository, IMapper mapper) : IRequestHandler<SendFriendRequestCommand>
{
    public async Task Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendRequest = mapper.Map<FriendRequest>(request.SendFriendRequestDTO);
        await repository.SendFriendRequest(friendRequest);
    }
}