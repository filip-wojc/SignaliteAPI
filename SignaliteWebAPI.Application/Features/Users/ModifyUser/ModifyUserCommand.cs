using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.ModifyUser;

public class ModifyUserCommand : IRequest
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
   
}