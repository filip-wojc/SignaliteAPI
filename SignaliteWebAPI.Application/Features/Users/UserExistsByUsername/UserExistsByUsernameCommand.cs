using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.UserExistsByUsername;

public class UserExistsByUsernameCommand : IRequest<bool>
{
    public required string Username { get; set; }
}