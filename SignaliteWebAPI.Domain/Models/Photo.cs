namespace SignaliteWebAPI.Domain.Models;

public class Photo
{
    public int Id { get; set; }
    
    public required string Url { get; set; }
    public required string PublicId { get; set; }
    
    public User User { get; set; }
    public int UserId { get; set; }
}