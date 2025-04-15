using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Friends.AcceptFriendRequest;
using SignaliteWebAPI.Application.Features.Friends.DeclineFriendRequest;
using SignaliteWebAPI.Application.Features.Friends.GetFriendRequests;
using SignaliteWebAPI.Application.Features.Friends.GetUserFriends;
using SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;
using SignaliteWebAPI.Domain.DTOs.Users;
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
            SenderId = User.GetUserId(),
            SenderUsername = User.GetUsername(),
            RecipientId = recipientId, 
        };
        await mediator.Send(command);
        return Created();
    }

    [HttpGet("friend-requests")]
    public async Task<ActionResult<List<FriendRequestDTO>>> GetFriendRequests()
    {
        var query = new GetFriendRequestsQuery { UserId = User.GetUserId() };
        var friendRequests = await mediator.Send(query);
        return Ok(friendRequests);
    }

    [HttpPost("friend-request/accept/{friendRequestId}")]
    public async Task<IActionResult> AcceptFriendRequest([FromRoute] int friendRequestId)
    {
        var command = new AcceptFriendRequestCommand
        {
            UserId = User.GetUserId(),
            FriendRequestId = friendRequestId 
        };
        await mediator.Send(command);
        return Created();
    }

    [HttpDelete("friend-request/decline/{friendRequestId}")]
    public async Task<IActionResult> DeclineFriendRequest([FromRoute] int friendRequestId)
    {
        var command = new DeclineFriendRequestCommand
        {
            UserId = User.GetUserId(),
            FriendRequestId = friendRequestId
        };
        await mediator.Send(command);
        return NoContent();
    }
    [HttpGet]
    public async Task<ActionResult<List<UserBasicInfo>>> GetUserFriends()
    {
        var query = new GetUserFriendsQuery{ UserId = User.GetUserId() };
        var friends = await mediator.Send(query);
        return Ok(friends);
    }
}