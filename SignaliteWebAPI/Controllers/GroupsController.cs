using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;
using SignaliteWebAPI.Application.Features.Groups.CreateGroup;
using SignaliteWebAPI.Application.Features.Groups.DeleteGroup;
using SignaliteWebAPI.Application.Features.Groups.GetGroupBasicInfo;
using SignaliteWebAPI.Application.Features.Groups.GetGroupMembers;
using SignaliteWebAPI.Application.Features.Groups.GetGroups;
using SignaliteWebAPI.Application.Features.Groups.RemoveUserFromGroup;
using SignaliteWebAPI.Application.Features.Groups.ModifyGroupName;
using SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Infrastructure.Extensions;


namespace SignaliteWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromQuery] string groupName)
    {
        var command = new CreateGroupCommand
        {
            Name = groupName,
            OwnerId = User.GetUserId()
        };
        await mediator.Send(command);
        return Created();
    }
    
    [HttpPut("{groupId}")]
    public async Task<IActionResult> ModifyGroupName([FromRoute] int groupId, string groupName)
    {
        var command = new ModifyGroupCommand
        {
            UserId = User.GetUserId(),
            GroupId = groupId,
            GroupName = groupName
        };
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("photo/{groupId}")]
    [EnableRateLimiting("file-upload")]
    public async Task<IActionResult> UpdateGroupPhoto(IFormFile file, [FromRoute] int groupId)
    {
        var command = new UpdateGroupPhotoCommand
        {
            GroupId = groupId,
            OwnerId = User.GetUserId(),
            Photo = file
        };
        await mediator.Send(command);
        return Created();
    }
    
    [HttpGet("{groupId}/basic-info")]
    public async Task<ActionResult<GroupBasicInfoDTO>> GetGroupBasicInfo([FromRoute] int groupId)
    {
        var query = new GetGroupBasicInfoQuery()
        {
            UserId = User.GetUserId(),
            GroupId = groupId
        };
        var group = await mediator.Send(query);
        return Ok(group);
    }
    
    [HttpGet("{groupId}/members")]
    public async Task<ActionResult<GroupMembersDTO>> GetGroupMembers([FromRoute] int groupId)
    {
        var query = new GetGroupMembersQuery()
        {
            UserId = User.GetUserId(),
            GroupId = groupId
        };
        var group = await mediator.Send(query);
        return Ok(group);
    }

    [HttpGet]
    public async Task<ActionResult<List<GroupBasicInfoDTO>>> GetGroups()
    {
        var query = new GetGroupsQuery
        {
            UserId = User.GetUserId()
        };
        var groups = await mediator.Send(query);
        return Ok(groups);
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup([FromRoute] int groupId)
    {
        var command = new DeleteGroupCommand
        {
            GroupId = groupId,
            OwnerId = User.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{groupId}/users/{userId}")]
    public async Task<IActionResult> DeleteUserFromGroup([FromRoute] int groupId, [FromRoute] int userId)
    {
        var command = new RemoveUserFromGroupCommand
        {
            GroupId = groupId,
            UserId = userId,
            OwnerId = User.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }
    
    [HttpPost("{groupId}/users/{userId}")]
    public async Task<IActionResult> AddUserToGroup([FromRoute] int groupId, [FromRoute] int userId)
    {
        var command = new AddUserToGroupCommand
        {
            GroupId = groupId,
            UserId = userId,
            OwnerId = User.GetUserId()
        };
        await mediator.Send(command);
        return NoContent();
    }
}