using AutoMapper;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class UserAutoMapper : Profile
{
    public UserAutoMapper()
    {
        CreateMap<RegisterUserDTO, User>();
    }
}