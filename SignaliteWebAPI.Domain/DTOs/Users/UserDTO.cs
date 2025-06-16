namespace SignaliteWebAPI.Domain.DTOs.Users;

// This will be used when fetching he full user, for example to see the profile or something
public class UserDTO: IUserDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }
    public string ProfilePhotoUrl { get; set; }
    public string BackgroundPhotoUrl { get; set; }
}