using AutoMapper;
using SignaliteWebAPI.Application.Features.Users.SendFriendRequest;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class FriendsAutoMapper : Profile
{
    public FriendsAutoMapper()
    {
        CreateMap<SendFriendRequestCommand, FriendRequest>();
        CreateMap<FriendRequest, FriendRequestDTO>()
            .ForMember(f => f.RequestDate, o
                => o.MapFrom(f => f.RequestDate.ToString("yyyy/MM/dd HH:mm:ss")))
            .ForMember(f => f.SenderUsername, o
                => o.MapFrom(f => f.Sender.Username));
    }
}