using MediatR;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Application.Features.Users.UpdateProfilePhoto;

public class UpdateProfilePhotoCommand : IRequest
{
    public int UserId { get; set; }
    public IFormFile PhotoFile { get; set; }
}