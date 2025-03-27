using AutoMapper;
using SignaliteWebAPI.Application.Helpers;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class MessageAutoMapper : Profile
{
    public MessageAutoMapper()
    {
        CreateMap<SendMessageDTO, Message>();
    }
}