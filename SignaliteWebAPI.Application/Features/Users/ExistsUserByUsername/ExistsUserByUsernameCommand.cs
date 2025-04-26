using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.ExistsByUsername;

public class ExistsUserByUsernameCommand : IRequest<bool>
{
    public string Username { get; set; }
}