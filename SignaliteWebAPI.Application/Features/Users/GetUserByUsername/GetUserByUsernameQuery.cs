using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Users.GetUserByUsername;

public class GetUserByUsernameQuery : IRequest<UserDTO>
{
    public string Username { get; set; }
}