using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Users.GetUserInfo;

public class GetUserInfoCommand : IRequest<IUserDTO> 
{
    public int UserId { get; set; }
    public bool IsOwner { get; set; }
}