using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.DeleteBackgroundPhoto;

public class DeleteBackgroundPhotoCommand : IRequest<bool>
{
    public int UserId { get; set; }
}