using MediatR;
using Microsoft.AspNetCore.Mvc;
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
}