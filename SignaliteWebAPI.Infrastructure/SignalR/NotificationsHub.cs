using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Infrastructure.Extensions;
using ILogger = Serilog.ILogger;
namespace SignaliteWebAPI.Infrastructure.SignalR;

[Authorize]
public class NotificationsHub(PresenceTracker presenceTracker, ILogger logger) : Hub
{
    // Log who connects or any errors
    public override async Task OnConnectedAsync()
    {
        try
        {
            // Get the username from the claims
            var username = Context.User?.GetUsername();
            if (string.IsNullOrEmpty(username))
            {
                logger.Warning($"[NotificationsHub] Connection {Context.ConnectionId} attempted to connect without a valid username claim");
                throw new HubException("Cannot get user - no valid username claim found");
            }

            // Get the user ID from the claims
            var userId = Context.User?.GetUserId() ?? -1;
            if (userId <= 0)
            {
                logger.Warning($"[NotificationsHub] User {username} attempted to connect without a valid ID claim");
                throw new HubException("Cannot get user - no valid user ID claim found");
            }

            logger.Debug($"[NotificationsHub] User {username} (ID: {userId}) connected with connection {Context.ConnectionId}");
            
            // The user connection is already tracked by PresenceTracker when they connect to PresenceHub
            // No need to track connections again, but we can log this connection
            
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"[NotificationsHub] Error in OnConnectedAsync for connection {Context.ConnectionId}");
            throw;
        }
    }
    
    // log who disconnects or any errors
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var username = Context.User?.GetUsername();
            if (string.IsNullOrEmpty(username))
            {
                logger.Warning($"[NotificationsHub] Connection {Context.ConnectionId} disconnected without a valid username claim");
            }
            else
            {
                var userId = Context.User?.GetUserId();
                logger.Debug($"[NotificationsHub] User {username} (ID: {userId}) disconnected with connection {Context.ConnectionId}. Reason: {exception?.Message ?? "Normal disconnect"}");
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"[NotificationsHub] Error in OnDisconnectedAsync for connection {Context.ConnectionId}");
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}