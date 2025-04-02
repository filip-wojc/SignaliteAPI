using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Application.Features.Users.ChangePassword;

public class ChangePasswordCommand : IRequest
{
    public int UserId { get; set; }
    public ChangePasswordDTO ChangePasswordDto { get; set; }
}