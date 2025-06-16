using AutoMapper;
using SignaliteWebAPI.Domain.Attachments;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class AttachmentsAutoMapper : Profile
{
    public AttachmentsAutoMapper()
    {
        CreateMap<Attachment, AttachmentDTO>();
    }
}