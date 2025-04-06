using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Infrastructure.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Infrastructure.SignalR
{
    [Authorize]
    public class SignalingHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;
        private readonly ILogger _logger;

        // Dictionary to track SignalingHub connection IDs by username
        private static readonly ConcurrentDictionary<string, List<string>> UserConnections = new();

        public SignalingHub(PresenceTracker presenceTracker, ILogger logger)
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
                    _logger.Warning($"[SignalingHub] Connection {Context.ConnectionId} attempted to connect without a valid username claim");
                    throw new HubException("Cannot get user - no valid username claim found");
                }

                // Get the user ID from the claims
                var userId = Context.User?.GetUserId() ?? -1;
                if (userId <= 0)
                {
                    _logger.Warning($"[SignalingHub] User {username} attempted to connect without a valid ID claim");
                    throw new HubException("Cannot get user - no valid user ID claim found");
                }

                // Add this connection to the user's list of SignalingHub connections
                lock (UserConnections)
                {
                    if (!UserConnections.TryGetValue(username, out var connections))
                    {
                        connections = new List<string>();
                        UserConnections[username] = connections;
                    }

                    connections.Add(Context.ConnectionId);
                }
                
                _logger.Debug($"[SignalingHub] User {username} (ID: {userId}) connected with connection {Context.ConnectionId}");
                
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SignalingHub] Error in OnConnectedAsync for connection {Context.ConnectionId}");
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
                    _logger.Warning($"[SignalingHub] Connection {Context.ConnectionId} disconnected without a valid username claim");
                }
                else
                {
                    var userId = Context.User?.GetUserId();
                    _logger.Debug($"[SignalingHub] User {username} (ID: {userId}) disconnected with connection {Context.ConnectionId}. Reason: {exception?.Message ?? "Normal disconnect"}");

                    // Remove this connection from the user's connections
                    lock (UserConnections)
                    {
                        if (UserConnections.TryGetValue(username, out var connections))
                        {
                            connections.Remove(Context.ConnectionId);
                            if (UserConnections[username].Count == 0)
                            {
                                UserConnections.TryRemove(username, out _);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SignalingHub] Error in OnDisconnectedAsync for connection {Context.ConnectionId}");
            }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }

        /// <summary>
        /// Gets all connection IDs for a username within the SignalingHub
        /// </summary>
        public static List<string> GetConnectionsForUsername(string username)
        {
            lock (UserConnections)
            {
                if (UserConnections.TryGetValue(username, out var connections))
                {
                    return new List<string>(connections); // Return a copy for thread safety
                }
                return [];
            }
        }

        /// <summary>
        /// Register the current client's connection with metadata for WebRTC
        /// </summary>
        public async Task RegisterForSignaling()
        {
            var username = Context.User?.GetUsername();
            if (string.IsNullOrEmpty(username))
            {
                throw new HubException("Cannot identify user");
            }

            var userId = Context.User?.GetUserId() ?? -1;
            if (userId <= 0)
            {
                throw new HubException("Cannot identify user ID");
            }

            // Return this connection's ID and information about online users
            var connectionsInfo = new
            {
                ConnectionId = Context.ConnectionId,
                OnlineUsers = await _presenceTracker.GetOnlineUsersDetailed() // TODO: REMOVE LATER
            };

            await Clients.Caller.SendAsync("SignalingRegistered", connectionsInfo);
            _logger.Debug($"[SignalingHub] User {username} (ID: {userId}) registered for signaling with connection {Context.ConnectionId}");
        }

        /// <summary>
        /// Sends an offer from the caller to a specific user
        /// </summary>
        /// <param name="targetUserId">The target user's ID</param>
        /// <param name="offer">The SDP offer in JSON format</param>
        public async Task SendOffer(int targetUserId, string offer)
        {
            try
            {
                var callerUsername = Context.User?.GetUsername();
                var callerId = Context.User?.GetUserId() ?? -1;
                
                if (string.IsNullOrEmpty(callerUsername) || callerId <= 0)
                {
                    _logger.Warning($"[SignalingHub] Invalid caller attempted to send offer to user {targetUserId}");
                    throw new HubException("Cannot identify caller");
                }

                _logger.Debug($"[SignalingHub] User {callerUsername} (ID: {callerId}) is sending offer to user ID {targetUserId}");

                // Get online users to find the target's username
                var onlineUsers = await _presenceTracker.GetOnlineUsersDetailed();
                var targetUser = onlineUsers.FirstOrDefault(u => u.Id == targetUserId);
                
                if (targetUser == null)
                {
                    _logger.Warning($"[SignalingHub] Target user ID {targetUserId} not found online");
                    return;
                }
                
                // Include caller info with the offer
                var offerData = new
                {
                    CallerUsername = callerUsername,
                    CallerId = callerId,
                    Offer = offer,
                    SourceConnectionId = Context.ConnectionId // Include the source connection ID
                };

                // Send to the user using the username
                await Clients.User(targetUser.Username).SendAsync("ReceiveOffer", offerData); // sends to all active connections
                _logger.Debug($"[SignalingHub] Sent offer from {callerUsername} to {targetUser.Username}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SignalingHub] Error in SendOffer method for connection {Context.ConnectionId}");
                throw;
            }
        }

        /// <summary>
        /// Sends an answer from a callee back to the caller
        /// </summary>
        /// <param name="targetUsername">The username of the caller</param>
        /// <param name="targetConnectionId">The specific connection ID of the caller that answered</param>
        /// <param name="answer">The SDP answer in JSON format</param>
        public async Task SendAnswer(string targetUsername, string targetConnectionId, string answer)
        {
            try
            {
                var calleeUsername = Context.User?.GetUsername();
                var calleeId = Context.User?.GetUserId() ?? -1;
                
                if (string.IsNullOrEmpty(calleeUsername) || calleeId <= 0)
                {
                    _logger.Warning($"[SignalingHub] Invalid callee attempted to send answer to {targetUsername}");
                    throw new HubException("Cannot identify callee");
                }

                _logger.Debug($"[SignalingHub] User {calleeUsername} (ID: {calleeId}) is sending answer to {targetUsername} (connection {targetConnectionId})");

                // Include callee info with the answer
                var answerData = new
                {
                    CalleeUsername = calleeUsername,
                    CalleeId = calleeId,
                    Answer = answer,
                    SourceConnectionId = Context.ConnectionId,  // Source connection ID (this connection)
                    TargetConnectionId = targetConnectionId     // Target connection ID (for filtering on client)
                };

                // Send to the user using the username
                await Clients.User(targetUsername).SendAsync("ReceiveAnswer", answerData);
                _logger.Debug($"[SignalingHub] Sent answer from {calleeUsername} to {targetUsername}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SignalingHub] Error in SendAnswer method for connection {Context.ConnectionId}");
                throw;
            }
        }

        /// <summary>
        /// Sends an ICE candidate to a peer
        /// </summary>
        /// <param name="targetUsername">The username of the peer</param>
        /// <param name="targetConnectionId">The specific connection ID of the peer</param>
        /// <param name="candidate">The ICE candidate in JSON format</param>
        public async Task SendIceCandidate(string targetUsername, string targetConnectionId, string candidate)
        {
            try
            {
                var username = Context.User?.GetUsername();
                var userId = Context.User?.GetUserId() ?? -1;
                
                if (string.IsNullOrEmpty(username) || userId <= 0)
                {
                    _logger.Warning($"[SignalingHub] Invalid user attempted to send ICE candidate to {targetUsername}");
                    throw new HubException("Cannot identify sender");
                }

                _logger.Debug($"[SignalingHub] User {username} (ID: {userId}) is sending ICE candidate to {targetUsername} (connection {targetConnectionId})");

                // Include sender info with the candidate
                var candidateData = new
                {
                    SenderUsername = username,
                    SenderId = userId,
                    Candidate = candidate,
                    SourceConnectionId = Context.ConnectionId,   // Source connection ID (this connection)
                    TargetConnectionId = targetConnectionId      // Target connection ID (for filtering on client)
                };

                // Send to the user using the username
                await Clients.User(targetUsername).SendAsync("ReceiveIceCandidate", candidateData);
                _logger.Debug($"[SignalingHub] Sent ICE candidate from {username} to {targetUsername}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SignalingHub] Error in SendIceCandidate method for connection {Context.ConnectionId}");
                throw;
            }
        }

        /// <summary>
        /// Handles call hangup
        /// </summary>
        /// <param name="targetUsername">The username of the peer to notify</param>
        /// <param name="targetConnectionId">The specific connection ID of the peer</param>
        public async Task HangUp(string targetUsername, string targetConnectionId)
        {
            try
            {
                var username = Context.User?.GetUsername();
                var userId = Context.User?.GetUserId() ?? -1;
                
                if (string.IsNullOrEmpty(username) || userId <= 0)
                {
                    _logger.Warning($"[SignalingHub] Invalid user attempted to hang up call with {targetUsername}");
                    throw new HubException("Cannot identify sender");
                }

                _logger.Debug($"[SignalingHub] User {username} (ID: {userId}) is hanging up call with {targetUsername} (connection {targetConnectionId})");

                // Send hangup info including connection IDs for client-side filtering
                await Clients.User(targetUsername).SendAsync("CallEnded", new { 
                    Username = username, 
                    UserId = userId,
                    SourceConnectionId = Context.ConnectionId,
                    TargetConnectionId = targetConnectionId
                });
                
                _logger.Debug($"[SignalingHub] Sent hangup from {username} to {targetUsername}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SignalingHub] Error in HangUp method for connection {Context.ConnectionId}");
                throw;
            }
        }
    }
}