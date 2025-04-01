using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.DTOs.Groups;

// will be used to add group to the group list instantly without fetching members and owner info
// the details info will be loaded when group is clicked
// also should be used to get a list of groups without fetching unnecessary details like members too soon
public class GroupBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PhotoUrl { get; set; }
    // TODO: last message from group
}