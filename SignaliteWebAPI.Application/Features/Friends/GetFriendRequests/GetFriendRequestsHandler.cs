using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Friends.GetFriendRequests;

public class GetFriendRequestsHandler(IFriendsRepository repository, IMapper mapper) : IRequestHandler<GetFriendRequestsQuery, List<FriendRequestDTO>>
{
    public async Task<List<FriendRequestDTO>> Handle(GetFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var friendRequests = await repository.GetFriendRequests(request.UserId);
        var friendRequestsDtos = mapper.Map<List<FriendRequestDTO>>(friendRequests);
        return friendRequestsDtos;
    }
}