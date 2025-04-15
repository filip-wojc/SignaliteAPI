using SignaliteWebAPI.Domain.DTOs.Users;

namespace SignaliteWebAPI.Domain.DTOs.Groups;

// this will be used to get the details like members and owner
public class GroupMembersDTO
{
    public UserBasicInfo Owner { get; set; }
    public List<UserBasicInfo> Members { get; set; } = [];
}