using API.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using SignaliteWebAPI.Infrastructure.Extensions;

namespace SignaliteWebAPI.Infrastructure.SignalR;

[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _presenceTracker;

    public PresenceHub(PresenceTracker presenceTracker)
    {
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.GetUsername();
        if (username == null)
            throw new HubException("Cannot get current user claims");

        var isOnline = await _presenceTracker.UserConnected(username, Context.ConnectionId);

        if (isOnline)
            await Clients.Others.SendAsync("UserIsOnline", username);

        var currentUsers = await _presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.GetUsername();
        if (username == null)
            throw new HubException("Cannot get current user claims");

        var isOffline = await _presenceTracker.UserDisconnected(username, Context.ConnectionId);

        if (isOffline)
            await Clients.Others.SendAsync("UserIsOffline", username);

        await base.OnDisconnectedAsync(exception);
    }
}