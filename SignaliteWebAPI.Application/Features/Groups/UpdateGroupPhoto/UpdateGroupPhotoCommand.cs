using MediatR;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;

public class UpdateGroupPhotoCommand : IRequest
{
    public int OwnerId { get; set; }
    public int GroupId { get; set; }
    public IFormFile Photo { get; set; }
}