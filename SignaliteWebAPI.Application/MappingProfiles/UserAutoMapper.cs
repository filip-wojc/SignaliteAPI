using AutoMapper;
using SignaliteWebAPI.Domain.DTOs.Auth;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class UserAutoMapper : Profile
{
    public UserAutoMapper()
    {
        CreateMap<RegisterDTO, User>();
        CreateMap<User, UserDTO>()
            .ForMember(u => u.ProfilePhotoUrl, o 
                => o.MapFrom(u => u.ProfilePhoto.Url));
        CreateMap<User, UserBasicInfo>();
    }
}