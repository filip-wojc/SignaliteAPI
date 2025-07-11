﻿using AutoMapper;
using SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;
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
                => o.MapFrom(f => f.Sender.Username))
            .ForMember(f => f.ProfilePhotoUrl, o
                => o.MapFrom(f => f.Sender.ProfilePhoto.Url));
    }
}