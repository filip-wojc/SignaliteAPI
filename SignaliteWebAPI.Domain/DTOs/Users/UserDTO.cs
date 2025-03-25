namespace SignaliteWebAPI.Domain.DTOs.Users;

public class UserDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }
    public string ProfilePhotoUrl { get; set; }
}