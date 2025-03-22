using MediatR;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;

public class UpdateBackgroundPhotoCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public IFormFile PhotoFile { get; set; }
}