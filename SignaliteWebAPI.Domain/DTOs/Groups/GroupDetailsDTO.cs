using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Domain.DTOs.Groups;

// this will be used to get the details like members and owner
public class GroupDetailsDTO
{
    public int Id { get; set; } // TODO: delete this later, use GroupBasicInfo when fetching a group list
    public string Name { get; set; } // TODO: delete this later, use GroupBasicInfo when fetching a group list
    public UserDTO Owner { get; set; } 
    public List<UserDTO> Members { get; set; }
    public string? PhotoUrl { get; set; } // TODO: delete this later, use GroupBasicInfo when fetching a group list
}