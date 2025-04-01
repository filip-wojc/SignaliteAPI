namespace SignaliteWebAPI.Domain.Models;

public class Group
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsPrivate { get; set; }
    public Photo? Photo { get; set; }
    public int? PhotoId { get; set; }
    public User Owner { get; set; }
    public required int OwnerId { get; set; }
    public List<UserGroup> Users { get; set; } = [];
}