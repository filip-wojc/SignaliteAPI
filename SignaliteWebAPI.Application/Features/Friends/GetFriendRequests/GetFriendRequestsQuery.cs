using MediatR;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Application.Features.Friends.GetFriendRequests;

public class GetFriendRequestsQuery : IRequest<List<FriendRequestDTO>>
{
    public int UserId { get; set; }
}