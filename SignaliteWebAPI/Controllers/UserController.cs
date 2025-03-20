using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Extensions;


namespace SignaliteWebAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController(ISender mediator) : ControllerBase
{
    // test endpoint to get id and username from claims
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        string id = User.GetUserId();
        string username = User.GetUsername();
        return Ok(new {id = id, username = username});
    }

    
}