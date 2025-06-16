using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Users.ModifyUser;

public class ModifyUserCommand : IRequest
{
    public int UserId { get; set; }
    public ModifyUserDTO ModifyUserDTO { get; set; }
   
}