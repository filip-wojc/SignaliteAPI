namespace SignaliteWebAPI.Domain.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public required string PasswordSalt { get; set; }
    public string? PhotoUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public List<UserGroup> UserGroups { get; set; } = [];
}