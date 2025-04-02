using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Users.GetUserInfo;

public class GetUserInfoCommand : IRequest<UserDTO> 
{
    public int UserId { get; set; }
}