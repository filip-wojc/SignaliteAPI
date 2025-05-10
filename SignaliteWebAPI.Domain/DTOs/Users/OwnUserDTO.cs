namespace SignaliteWebAPI.Domain.DTOs.Users;

public class OwnUserDTO: UserDTO, IUserDTO{
    public string Email { get; set; }
}