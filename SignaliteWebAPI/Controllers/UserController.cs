using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.User.AddUser;


namespace SignaliteWebAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController(ISender mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(AddUserCommand addUserCommand)
    {
        await mediator.Send(addUserCommand);
        return Created();
    }
}