using MediatR;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;

public class UpdateBackgroundPhotoCommand : IRequest
{
    public int UserId { get; set; }
    public IFormFile PhotoFile { get; set; }
}