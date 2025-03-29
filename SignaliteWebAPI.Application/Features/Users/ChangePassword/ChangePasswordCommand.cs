using MediatR;

namespace SignaliteWebAPI.Application.Features.Users.ChangePassword;

public class ChangePasswordCommand : IRequest
{
    public int UserId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}