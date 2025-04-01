using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Domain.DTOs.Groups;

public class GroupDetailsDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserDTO Owner { get; set; }
    public List<UserDTO> Members { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsPrivate { get; set; }
}