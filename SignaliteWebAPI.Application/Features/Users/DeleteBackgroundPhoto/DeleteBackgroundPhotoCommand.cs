using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.DeleteBackgroundPhoto;

public class DeleteBackgroundPhotoCommand : IRequest
{
    public int UserId { get; set; }
}