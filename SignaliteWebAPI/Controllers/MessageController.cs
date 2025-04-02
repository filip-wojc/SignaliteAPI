using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Application.Features.Messages.DeleteMessage;
using SignaliteWebAPI.Application.Features.Messages.GetMessageThread;
using SignaliteWebAPI.Application.Features.Messages.SendMessage;
using SignaliteWebAPI.Application.Helpers;
using SignaliteWebAPI.Domain.DTOs.Messages;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Extensions;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

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

    [HttpGet("{groupId}")]
    public async Task<ActionResult<List<MessageDTO>>> GetMessageThread([FromRoute] int groupId)
    {
        var query = new GetMessageThreadQuery
        {
            GroupId = groupId,
            UserId = User.GetUserId()
        };
        var messages = await mediator.Send(query);
        return Ok(messages);
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage([FromRoute] int messageId)
    {
        var command = new DeleteMessageCommand
        {
            MessageId = messageId,
            SenderId = User.GetUserId(),
        };
        await mediator.Send(command);
        return NoContent();
    }
}