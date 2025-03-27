using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Messages.SendMessage;
using SignaliteWebAPI.Application.Helpers;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Extensions;

namespace SignaliteWebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageDTO messageDto)
    {
        var command = new SendMessageCommand
        {
            SendMessageDto = messageDto,
            SenderId = User.GetUserId(),
        };
        await mediator.Send(command);
        return Created();
    }
}