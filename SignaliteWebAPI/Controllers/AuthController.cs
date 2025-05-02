using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Auth.ExistsUserByEmail;
using SignaliteWebAPI.Application.Features.Auth.ExistsUserByUsername;
using SignaliteWebAPI.Application.Features.Auth.Login;
using SignaliteWebAPI.Application.Features.Auth.RefreshToken;
using SignaliteWebAPI.Application.Features.Auth.Register;
using SignaliteWebAPI.Domain.DTOs.Auth;

namespace SignaliteWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(RegisterCommand registerCommand)
    {
        await mediator.Send(registerCommand);
        return Created();
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDTO>> Login(LoginCommand loginCommand)
    {
        var tokenResponse = await mediator.Send(loginCommand);
        return Ok(tokenResponse);
    }
    
    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenCommand refreshCommand)
    {
        var tokenResponse = await mediator.Send(refreshCommand);
        return Ok(tokenResponse);
    }
    
    [HttpGet("user-exists-by-username")]
    public async Task<ActionResult> ExistsUserByUsername(string username)
    {
        var command = new ExistsUserByUsernameCommand
        {
            Username = username
        };
        var content = await mediator.Send(command);
        return Ok(content);
    }
    
    [HttpGet("user-exists-by-email")]
    public async Task<ActionResult> ExistsUserByEmail(string email)
    {
        var command = new ExistsUserByEmailCommand
        {
            Email = email
        };
        var content = await mediator.Send(command);
        return Ok(content);
    }
}