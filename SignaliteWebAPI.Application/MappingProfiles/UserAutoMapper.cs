using AutoMapper;
using SignaliteWebAPI.Domain.DTOs.Auth;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class UserAutoMapper : Profile
{
    public UserAutoMapper()
    {
        CreateMap<RegisterDTO, User>();
    }
}