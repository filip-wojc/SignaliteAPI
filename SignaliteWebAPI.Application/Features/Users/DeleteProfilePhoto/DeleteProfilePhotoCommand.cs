using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.DeleteProfilePhoto;

public class DeleteProfilePhotoCommand : IRequest<bool>
{
    public int UserId { get; set; }
}