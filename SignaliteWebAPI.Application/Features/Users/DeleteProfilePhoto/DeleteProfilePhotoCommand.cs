using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.DeleteProfilePhoto;

public class DeleteProfilePhotoCommand : IRequest
{
    public int UserId { get; set; }
}