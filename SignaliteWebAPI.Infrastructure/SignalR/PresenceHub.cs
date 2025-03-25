using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Infrastructure.Extensions;
using System;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Infrastructure.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;
        private readonly ILogger _logger;

        public PresenceHub(PresenceTracker presenceTracker, ILogger logger)
        {
            _presenceTracker = presenceTracker;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var username = Context.User?.GetUsername();
                if (string.IsNullOrEmpty(username))
                    throw new HubException("Cannot get current user claims");

                _logger.Debug($"User {username} connected with connection {Context.ConnectionId}");
                
                var isOnline = await _presenceTracker.UserConnected(username, Context.ConnectionId);

                if (isOnline)
                {
                    // Broadcast the online status only if this is the first connection for this user
                    await Clients.Others.SendAsync("UserIsOnline", username);
                }

                var currentUsers = await _presenceTracker.GetOnlineUsers();
                await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in OnConnectedAsync for connection {Context.ConnectionId}");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <exception cref="HubException"></exception>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var username = Context.User?.GetUsername();
                if (string.IsNullOrEmpty(username))
                    throw new HubException("Cannot get current user claims");

                _logger.Debug($"User {username} disconnected with connection {Context.ConnectionId}. Reason: {exception?.Message ?? "Normal disconnect"}");
                
                var isOffline = await _presenceTracker.UserDisconnected(username, Context.ConnectionId);

                if (isOffline)
                {
                    // Broadcast the offline status only if this was the last connection for this user
                    await Clients.Others.SendAsync("UserIsOffline", username);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in OnDisconnectedAsync for connection {Context.ConnectionId}");
            }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }
        
        // Add a handler for the keep-alive message
        public async Task KeepAliveResponse(DateTime timestamp)
        {
            var username = Context.User?.GetUsername();
            if (!string.IsNullOrEmpty(username))
            {
                _logger.Debug($"Received keep-alive response from {username} on connection {Context.ConnectionId}");
            
                // Pass to presence tracker to handle validation completion
                _presenceTracker.HandleKeepAliveResponse(Context.ConnectionId);
            }
        
            await Task.CompletedTask;
        }
    }
}