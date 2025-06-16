using MediatR;

namespace SignaliteWebAPI.Application.Features.Auth.ExistsUserByUsername;

public class ExistsUserByUsernameCommand : IRequest<bool>
{
    public string Username { get; set; }
}