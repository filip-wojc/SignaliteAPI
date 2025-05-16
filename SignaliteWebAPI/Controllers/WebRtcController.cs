using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Infrastructure.Services;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WebRtcController(WebRtcConfigService webRtcConfigService, ILogger logger) : ControllerBase
{
    private readonly WebRtcConfigService _webRtcConfigService = webRtcConfigService;
    private readonly ILogger _logger = logger;
    
    /// <summary>
    /// Gets the configured ICE servers for WebRTC
    /// </summary>
    [HttpGet("ice-servers")]
    public IActionResult GetIceServers()
    {
        try
        {
            var iceServers = _webRtcConfigService.GetIceServers();
            return Ok(iceServers);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Error retrieving ICE servers");
            return StatusCode(500, "Error retrieving ICE servers configuration");
        }
    }
}