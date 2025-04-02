namespace SignaliteWebAPI.Domain.DTOs.Users;

public class ChangePasswordDTO
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}