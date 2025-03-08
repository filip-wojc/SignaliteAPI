namespace SignaliteWebAPI.Domain.Models;

public class UserGroup
{
    public int Id { get; set; }
    public User User { get; set; }
    public required int UserId { get; set; }
    public Group Group { get; set; }
    public required int GroupId { get; set; }
}