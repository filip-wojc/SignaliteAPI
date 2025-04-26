using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.ExistsByEmail;

public class ExistsUserByEmailCommand : IRequest<bool>
{
    public string Email { get; set; }
}