using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Users.GetFriendRequests;
using SignaliteWebAPI.Application.Features.Users.SendFriendRequest;
using SignaliteWebAPI.Domain.DTOs.FriendRequests;

namespace SignaliteWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendsController(ISender mediator) : ControllerBase
{
    [HttpPost("friend-request")]
    public async Task<IActionResult> SendFriendRequest(SendFriendRequestCommand sendFriendRequestCommand)
    {
        await mediator.Send(sendFriendRequestCommand);
        return Created();
    }

    // Will be replaced with user id from token
    [HttpGet("friend-requests/{userId}")]
    public async Task<ActionResult<List<FriendRequestDTO>>> GetFriendRequests([FromRoute] int userId)
    {
        var friendRequests = await mediator.Send(new GetFriendRequestsQuery { UserId = userId });
        return Ok(friendRequests);
    }
}