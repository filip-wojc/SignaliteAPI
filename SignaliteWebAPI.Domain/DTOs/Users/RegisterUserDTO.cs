using System.ComponentModel.DataAnnotations;

namespace SignaliteWebAPI.Domain.DTOs.Users;

public class RegisterUserDTO
{
    public string Username { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}