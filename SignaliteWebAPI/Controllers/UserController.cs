using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Users.AddProfilePhoto;
using SignaliteWebAPI.Application.Features.Users.DeleteBackgroundPhoto;
using SignaliteWebAPI.Application.Features.Users.DeleteProfilePhoto;
using SignaliteWebAPI.Application.Features.Users.GetUserInfo;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
using SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;
using SignaliteWebAPI.Extensions;
using SignaliteWebAPI.Infrastructure.Database.Migrations;
using SignaliteWebAPI.Infrastructure.Extensions;
using SignaliteWebAPI.Infrastructure.Interfaces;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;


namespace SignaliteWebAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController(ISender mediator, IMediaService mediaService) : ControllerBase
{
    [Authorize]
    [HttpPut("modify-user")]
    public async Task<IActionResult> ModifyUser(string username, string email,string name,string surname)
    {
        var userId = User.GetUserId();
        var command = new ModifyUserCommand
        {
            UserId = userId,
            Username = username,
            Email = email,
            Name = name,
            Surname = surname
        };
        await mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpGet("get-user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.GetUserId();
        var command = new GetUserInfoCommand
        {
            UserId = userId
        };
        var content = await mediator.Send(command);
        return Ok(content);
    }
    
    // Tested: works
    [Authorize]
    [HttpPost("profile-photo")]
    public async Task<ActionResult<string>> UpdateProfilePhoto(IFormFile file)
    {
        var userId = User.GetUserId();
        var command = new UpdateProfilePhotoCommand
        {
            UserId = userId,
            PhotoFile = file,
        };
        await mediator.Send(command);
        return Created();
    }
    
    // Tested: also works
    [Authorize]
    [HttpDelete("profile-photo")]
    public async Task<IActionResult> DeleteProfilePhoto()
    {
        var userId = User.GetUserId();
        var command = new DeleteProfilePhotoCommand
        {
            UserId = userId
        };
    
        await mediator.Send(command);
        return NoContent();

    }

    // tested: works
    [Authorize]
    [HttpPost("bg-photo")]
    public async Task<IActionResult> UpdateBackgroundPhoto(IFormFile file)
    {
        var userId = User.GetUserId();
        var command = new UpdateBackgroundPhotoCommand()
        {
            UserId = userId,
            PhotoFile = file,
        };
        await mediator.Send(command);
        return Created();
    }
    
    // tested: works
    [Authorize]
    [HttpDelete("bg-photo")]
    public async Task<IActionResult> DeleteBackgroundPhoto()
    {
        var userId = User.GetUserId();
        var command = new DeleteBackgroundPhotoCommand
        {
            UserId = userId
        };
    
        await mediator.Send(command); 
        
        return NoContent();
    }
    
}