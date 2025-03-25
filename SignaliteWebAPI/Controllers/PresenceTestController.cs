using System.Collections.Generic;
using System.Threading.Tasks;
using SignaliteWebAPI.Infrastructure.SignalR;
using Microsoft.AspNetCore.Mvc;
using SignaliteWebAPI.Infrastructure.Extensions;
using SignaliteWebAPI.Infrastructure.SignalR;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresenceTestController : ControllerBase
    {
        private readonly PresenceTracker _presenceTracker;

        public PresenceTestController(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        [HttpGet("connect")]
        public async Task<IActionResult> TestConnect(string username)
        {
            // Simulate a connection
            var userID = User.GetUserId();
            var connectionId = "test-connection-" + System.Guid.NewGuid().ToString();
            var isOnline = await _presenceTracker.UserConnected(username, connectionId, userID);
            
            return Ok(new { 
                username, 
                connectionId, 
                isOnline, 
                message = isOnline ? "User is now online" : "User was already online" 
            });
        }

        [HttpGet("disconnect")]
        public async Task<IActionResult> TestDisconnect(string username, string connectionId)
        {
            var isOffline = await _presenceTracker.UserDisconnected(username, connectionId);
            
            return Ok(new { 
                username, 
                isOffline, 
                message = isOffline ? "User is now offline" : "User still has other connections" 
            });
        }

        [HttpGet("online-users")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var onlineUsers = await _presenceTracker.GetOnlineUserIds();
            
            return Ok(new { users = onlineUsers, count = onlineUsers.Count() });
        }

        [HttpGet("user-connections")]
        public async Task<IActionResult> GetUserConnections(string username)
        {
            var connections = await _presenceTracker.GetConnectionsForUser(username);
            
            return Ok(new { username, connections, count = connections.Count });
        }
    }
}