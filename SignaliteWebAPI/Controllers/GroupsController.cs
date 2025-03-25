using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;
using SignaliteWebAPI.Application.Features.Groups.CreateGroup;
using SignaliteWebAPI.Application.Features.Groups.DeleteGroup;
using SignaliteWebAPI.Application.Features.Groups.DeleteUserFromGroup;
using SignaliteWebAPI.Application.Features.Groups.GetGroupDetails;
using SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Extensions;
using SignaliteWebAPI.Infrastructure.Extensions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

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

    [HttpPost("photo/{groupId}")]
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
    
    [HttpGet("{groupId}")]
    public async Task<ActionResult<Group>> GetGroupDetails([FromRoute] int groupId)
    {
        var query = new GetGroupDetailsQuery
        {
            GroupId = groupId
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
        var command = new DeleteUserFromGroupCommand
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