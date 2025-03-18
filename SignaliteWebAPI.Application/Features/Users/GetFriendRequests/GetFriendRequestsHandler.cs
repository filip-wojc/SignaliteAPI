using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Domain.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.GetFriendRequests;

public class GetFriendRequestsHandler(IFriendsRepository repository, IMapper mapper) : IRequestHandler<GetFriendRequestsQuery, List<GetFriendRequestDTO>>
{
    public async Task<List<GetFriendRequestDTO>> Handle(GetFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var friendRequests = await repository.GetFriendRequests(request.UserId);
        var friendRequestsDtos = mapper.Map<List<GetFriendRequestDTO>>(friendRequests);
        return friendRequestsDtos;
    }
}