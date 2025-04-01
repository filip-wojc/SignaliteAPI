namespace SignaliteWebAPI.Domain.DTOs.Users;

// This will be used to load the users into the members list or to send notification about
// new user that was just added, no need for name, surname and background photo if we're not displaying it
public class UserBasicInfo
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}