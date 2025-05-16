using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Infrastructure.Extensions;
using System;
using System.Threading.Tasks;
using Serilog;
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
                // Get the username from the claims
                var username = Context.User?.GetUsername();
                if (string.IsNullOrEmpty(username))
                {
                    _logger.Warning($"Connection {Context.ConnectionId} attempted to connect without a valid username claim");
                    throw new HubException("Cannot get user - no valid username claim found");
                }

                // Get the user ID from the claims
                var userId = Context.User?.GetUserId() ?? -1; // default case for protection
                if (userId <= 0)
                {
                    _logger.Warning($"User {username} attempted to connect without a valid ID claim");
                    throw new HubException("Cannot get user - no valid user ID claim found");
                }

                _logger.Debug($"User {username} (ID: {userId}) connected with connection {Context.ConnectionId}");
        
                var isOnline = await _presenceTracker.UserConnected(username, Context.ConnectionId, userId);

                var notification = new
                {
                    id = userId,
                    username = username,
                };
                
                if (isOnline)
                {
                    // Broadcast the online status only if this is the first connection for this user
                    // Include both username and userId in the notification
                    await Clients.Others.SendAsync("UserIsOnline", notification);
                }

                // Send the simple list of user IDs to the client
                var onlineUserIds = await _presenceTracker.GetOnlineUserIds();
                await Clients.Caller.SendAsync("GetOnlineUserIds", onlineUserIds);

        
                // Also send detailed user information for debugging purposes
                // TODO : REMOVE AFTER TESTING
                var onlineUsersDetailed = await _presenceTracker.GetOnlineUsersDetailed();
                await Clients.Caller.SendAsync("GetOnlineUsersDetailed", onlineUsersDetailed);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in OnConnectedAsync for connection {Context.ConnectionId}");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var username = Context.User?.GetUsername();
                if (string.IsNullOrEmpty(username))
                {
                    _logger.Warning($"Connection {Context.ConnectionId} disconnected without a valid username claim");
                    throw new HubException("Cannot get current user claims - no valid username found");
                }

                var userId = Context.User?.GetUserId();
                
                _logger.Debug($"User {username} (ID: {userId}) disconnected with connection {Context.ConnectionId}. Reason: {exception?.Message ?? "Normal disconnect"}");
                
                var isOffline = await _presenceTracker.UserDisconnected(username, Context.ConnectionId);

                var notification = new
                {
                    id = userId,
                    username = username,
                };
                
                if (isOffline)
                {
                    // Broadcast the offline status only if this was the last connection for this user
                    // Include both username and userId in the notification
                    await Clients.Others.SendAsync("UserIsOffline", notification);
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
                var userId = Context.User?.GetUserId();
                _logger.Debug($"Received keep-alive response from {username} (ID: {userId}) on connection {Context.ConnectionId}");
            
                // Pass to presence tracker to handle validation completion
                _presenceTracker.HandleKeepAliveResponse(Context.ConnectionId);
            }
            else
            {
                _logger.Warning($"Received keep-alive response from unknown user on connection {Context.ConnectionId}");
            }
        
            await Task.CompletedTask;
        }
    }
}