using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.User.AddUser;

public class AddUserCommand : IRequest
{
    public RegisterUserDTO RegisterUserDto { get; set; }
}