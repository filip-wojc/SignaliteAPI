using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;
using SignaliteWebAPI.Application.Features.Users.DeclineFriendRequest;
using SignaliteWebAPI.Application.Features.Users.GetFriendRequests;
using SignaliteWebAPI.Application.Features.Users.SendFriendRequest;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Extensions;
using SignaliteWebAPI.Infrastructure.Extensions;

namespace SignaliteWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController(ISender mediator) : ControllerBase
{
    [HttpPost("friend-request/{recipientId}")]
    public async Task<IActionResult> SendFriendRequest([FromRoute] int recipientId)
    {
        var command = new SendFriendRequestCommand
        {
            SendFriendRequestDTO = new SendFriendRequestDTO { RecipientId = recipientId, SenderId = User.GetUserId() }
        };
        await mediator.Send(command);
        return Created();
    }

    [HttpGet("friend-requests")]
    public async Task<ActionResult<List<FriendRequestDTO>>> GetFriendRequests()
    {
        var friendRequests = await mediator.Send(new GetFriendRequestsQuery { UserId = User.GetUserId() });
        return Ok(friendRequests);
    }

    [HttpPost("friend-request/accept/{friendRequestId}")]
    public async Task<IActionResult> AcceptFriendRequest([FromRoute] int friendRequestId)
    {
        var command = new AcceptFriendRequestCommand
        {
           AcceptFriendRequestReplyDto = new FriendRequestReplyDTO {UserId = User.GetUserId(), FriendRequestId = friendRequestId}
        };
        await mediator.Send(command);
        return Created();
    }

    [HttpDelete("friend-request/decline/{friendRequestId}")]
    public async Task<IActionResult> DeclineFriendRequest([FromRoute] int friendRequestId)
    {
        var command = new DeclineFriendRequestCommand
        {
            DeclineFriendRequestReplyDto = new FriendRequestReplyDTO { UserId = User.GetUserId(), FriendRequestId = friendRequestId }
        };
        await mediator.Send(command);
        return NoContent();
    }
}