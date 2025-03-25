using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Friends.GetUserFriends;

public class GetUserFriendsQuery : IRequest<List<UserDTO>>
{
    public int UserId { get; set; }
}