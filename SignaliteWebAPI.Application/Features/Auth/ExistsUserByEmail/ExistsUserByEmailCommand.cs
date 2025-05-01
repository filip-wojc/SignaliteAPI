using MediatR;

namespace SignaliteWebAPI.Application.Features.Auth.ExistsUserByEmail;

public class ExistsUserByEmailCommand : IRequest<bool>
{
    public string Email { get; set; }
}