using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.Features.Users.SendFriendRequest;

public class SendFriendRequestHandler(IFriendsRepository repository, IMapper mapper) : IRequestHandler<SendFriendRequestCommand>
{
    public async Task Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendRequest = mapper.Map<FriendRequest>(request.SendFriendRequestDTO);
        var userFriends = await repository.GetAllUserFriends();
        var isFriendAdded = userFriends.Where(uf =>
            (uf.UserId == request.SendFriendRequestDTO.RecipientId && uf.FriendId == request.SendFriendRequestDTO.SenderId) ||
            (uf.UserId == request.SendFriendRequestDTO.SenderId && uf.FriendId == request.SendFriendRequestDTO.RecipientId)
        ).Any();

        if (isFriendAdded)
        {
            throw new ForbidException("User already accepted friend request from sender");
        }
        await repository.SendFriendRequest(friendRequest);
    }
}