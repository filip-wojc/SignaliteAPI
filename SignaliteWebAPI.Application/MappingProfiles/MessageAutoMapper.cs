using AutoMapper;
using SignaliteWebAPI.Application.Helpers;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class MessageAutoMapper : Profile
{
    public MessageAutoMapper()
    {
        CreateMap<SendMessageDTO, Message>();
        CreateMap<Message, MessageDTO>()
            .ForMember(m => m.Sender, o => o.MapFrom(src => src.Sender))
            .ForMember(m => m.Attachment, o => o.MapFrom(src => src.Attachment))
            .ForMember(m => m.LastModification,
                o => o.MapFrom(src => src.DateModified ?? src.DateSent));
    }
}