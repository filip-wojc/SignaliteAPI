using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Auth.ExistsUserByEmail;
using SignaliteWebAPI.Application.Features.Auth.ExistsUserByUsername;
using SignaliteWebAPI.Application.Features.Users.ChangePassword;
using SignaliteWebAPI.Application.Features.Users.DeleteBackgroundPhoto;
using SignaliteWebAPI.Application.Features.Users.DeleteProfilePhoto;
using SignaliteWebAPI.Application.Features.Users.GetUserInfo;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
using SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Application.Features.Users.UpdateProfilePhoto;
using SignaliteWebAPI.Infrastructure.Extensions;

namespace SignaliteWebAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController(ISender mediator) : ControllerBase
{
    [Authorize]
    [HttpPut("modify-user")]
    public async Task<IActionResult> ModifyUser(ModifyUserDTO modifyUserDto)
    {
        var userId = User.GetUserId();
        
        var command = new ModifyUserCommand
        {
            UserId = userId,
            ModifyUserDTO = modifyUserDto
        };
        await mediator.Send(command);
        return NoContent();
    }
    
    [HttpGet("get-user-info")]
    public async Task<ActionResult> GetUserInfo(int? userId = null)
    {
        var resolvedUserId = userId ?? User.GetUserId();
        var command = new GetUserInfoCommand
        {
            UserId = resolvedUserId
        };
        var content = await mediator.Send(command);
        return Ok(content);
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDto)
    {
        var userId = User.GetUserId();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            ChangePasswordDto = changePasswordDto
        };
        await mediator.Send(command);
        return NoContent();
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