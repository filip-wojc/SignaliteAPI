using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
        int id = int.Parse(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);
        string username = User.FindFirst(c => c.Type == ClaimTypes.Name).Value;
        return Ok(new {id = id, username = username});
    }
}