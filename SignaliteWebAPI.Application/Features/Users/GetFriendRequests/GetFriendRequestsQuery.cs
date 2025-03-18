using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Users.GetFriendRequests;

public class GetFriendRequestsQuery : IRequest<List<GetFriendRequestDTO>>
{
    public int UserId { get; set; }
}