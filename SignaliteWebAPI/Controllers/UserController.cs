using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace SignaliteWebAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController(ISender mediator) : ControllerBase
{
    
}