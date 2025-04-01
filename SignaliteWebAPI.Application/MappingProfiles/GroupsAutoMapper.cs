using AutoMapper;
using SignaliteWebAPI.Application.Features.Groups.CreateGroup;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class GroupsAutoMapper : Profile
{
    public GroupsAutoMapper()
    {
        CreateMap<CreateGroupCommand, Group>();
        CreateMap<Group, GroupDetailsDTO>()
            .ForMember(g => g.Owner, o => o.MapFrom(g => g.Owner))
            .ForMember(g => g.Members,
                o => o.MapFrom(g => g.Users.Where(u => g.OwnerId != u.UserId).Select(u => u.User)))
            .ForMember(g => g.PhotoUrl, o => o.MapFrom(g => g.Photo.Url));
        CreateMap<Group, GroupBasicInfo>();
    }
}