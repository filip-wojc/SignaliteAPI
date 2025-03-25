using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Friends.GetUserFriends;

public class GetUserFriendsHandler(IFriendsRepository repository, IMapper mapper)
    : IRequestHandler<GetUserFriendsQuery, List<UserDTO>>
{
    public async Task<List<UserDTO>> Handle(GetUserFriendsQuery request, CancellationToken cancellationToken)
    {
        
        var friends = await repository.GetUserFriends(request.UserId);
        var friendsDto = mapper.Map<List<UserDTO>>(friends);
        return friendsDto;
    }
}