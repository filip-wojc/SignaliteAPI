using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignaliteWebAPI.Infrastructure.Helpers;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Infrastructure.SignalR
{
    public class PresenceTracker
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger _logger;

        // Key prefixes and constants
        private const string UserConnectionPrefix = "user:";
        private const string UserIdMappingPrefix = "userid:";
        private const string OnlineUsersKey = "online-users";
        private const string AppInstancesSetKey = "app:instances";
        private const string AppInstanceKeyPrefix = "app:instance:";
        private const string InstanceConnectionPrefix = "instance:connections:";

        // properties to track users connections being validated ( users need to send keep-alive response when asked)
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingValidations =
            new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

        private readonly TimeSpan _connectionValidationTimeout = TimeSpan.FromSeconds(10);

        // Timeouts and expiration times
        // How often each application instance refreshes its "I'm alive" marker in Redis
        private static readonly TimeSpan InstanceHeartbeatInterval = TimeSpan.FromMinutes(5);

        // How long an instance is considered alive without a heartbeat update
        private static readonly TimeSpan InstanceExpirationTime = TimeSpan.FromMinutes(15);

        // Failsafe expiration for user-related keys, ensures Redis doesn't accumulate stale data even if other cleanup mechanisms fail
        private static readonly TimeSpan KeyExpirationTime = TimeSpan.FromDays(1);

        private readonly string _instanceId;
        private readonly string _instanceKey;
        private readonly string _instanceConnectionsKey;

        public PresenceTracker(IConnectionMultiplexer redis, ILogger logger)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
            _instanceId = Guid.NewGuid().ToString();
            _instanceKey = $"{AppInstanceKeyPrefix}{_instanceId}";
            _instanceConnectionsKey = $"{InstanceConnectionPrefix}{_instanceId}";
            _logger = logger;
        }

        /// <summary>
        /// Registers this application instance in Redis with appropriate expiration
        /// </summary>
        public async Task RegisterInstance()
        {
            // Add this instance to the set of running instances
            await _db.SetAddAsync(AppInstancesSetKey, _instanceId);

            // Set the instance key with an expiration time
            await _db.StringSetAsync(_instanceKey, DateTime.UtcNow.ToString("o"), InstanceExpirationTime);

            // Create a set for tracking connections associated with this instance
            await _db.KeyExpireAsync(_instanceConnectionsKey, KeyExpirationTime);

            _logger.Debug($"Instance {_instanceId} registered");
        }

        /// <summary>
        /// Updates the heartbeat timestamp for this instance to indicate it's still alive
        /// </summary>
        public async Task UpdateInstanceHeartbeat()
        {
            await _db.StringSetAsync(_instanceKey, DateTime.UtcNow.ToString("o"), InstanceExpirationTime);
        }

        /// <summary>
        /// Unregisters this instance and cleans up its connections
        /// </summary>
        public async Task UnregisterInstance()
        {
            try
            {
                _logger.Information($"Unregistering instance {_instanceId}");

                // Clean up all connections for this instance
                await CleanupInstanceConnections(_instanceId);

                // Remove the instance from the instances set
                await _db.SetRemoveAsync(AppInstancesSetKey, _instanceId);

                // Delete the instance key
                await _db.KeyDeleteAsync(_instanceKey);

                _logger.Information($"Instance {_instanceId} unregistered successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error unregistering instance {_instanceId}");
            }
        }

        /// <summary>
        /// Marks a user as connected with the specified connection ID
        /// </summary>
        public async Task<bool> UserConnected(string username, string connectionId, int userId)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(username))
            {
                _logger.Warning("Attempted to connect user with null or empty username");
                return false;
            }

            if (userId <= 0)
            {
                _logger.Warning($"Attempted to connect user {username} with invalid userId: {userId}");
                return false;
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.Warning($"Attempted to connect user {username} with null or empty connectionId");
                return false;
            }

            var isOnline = false;
            var userKey = $"{UserConnectionPrefix}{username}";
            var userIdMapping = $"{UserIdMappingPrefix}{username}";
            var connectionInfo = $"{username}:{userId}:{connectionId}";

            try
            {
                // Store user ID mapping
                await _db.StringSetAsync(userIdMapping, userId.ToString(), KeyExpirationTime);

                // Add connection ID to user's set with expiration
                await _db.SetAddAsync(userKey, connectionId);
                await _db.KeyExpireAsync(userKey, KeyExpirationTime);

                // Add connection to this instance's set
                await _db.SetAddAsync(_instanceConnectionsKey, connectionInfo);

                // Check if this is the first connection for the user
                if (await _db.SetLengthAsync(userKey) == 1)
                {
                    // First connection, add to online users set with the format "username:userId"
                    await _db.SetAddAsync(OnlineUsersKey, $"{username}:{userId}");
                    isOnline = true;
                    _logger.Debug($"User {username} (ID: {userId}) is now online with connection {connectionId}");
                }
                else
                {
                    _logger.Debug($"Additional connection {connectionId} for user {username} (ID: {userId})");
                }

                // Update the instance heartbeat
                await UpdateInstanceHeartbeat();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error connecting user {username} (ID: {userId}) with connection {connectionId}");
            }

            return isOnline;
        }

        /// <summary>
        /// Marks a user as disconnected for the specified connection ID
        /// </summary>
        public async Task<bool> UserDisconnected(string username, string connectionId)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(connectionId))
            {
                _logger.Warning($"Attempted to disconnect user with invalid data. Username: {username}, ConnectionId: {connectionId}");
                return false;
            }

            var isOffline = false;
            var userKey = $"{UserConnectionPrefix}{username}";
            var userIdMapping = $"{UserIdMappingPrefix}{username}";

            try
            {
                // Get the userId from the mapping
                var userIdStr = await _db.StringGetAsync(userIdMapping);
                int userId = 0;
                
                if (!userIdStr.IsNullOrEmpty)
                {
                    int.TryParse(userIdStr.ToString(), out userId);
                }

                if (userId <= 0)
                {
                    _logger.Warning($"Could not find valid userId for username {username} during disconnection");
                }

                // Remove connection ID from user's set
                await _db.SetRemoveAsync(userKey, connectionId);

                // Remove connection from instance connections (find all matching connections)
                var allConnections = await _db.SetMembersAsync(_instanceConnectionsKey);
                foreach (var conn in allConnections)
                {
                    var connStr = conn.ToString();
                    if (connStr.Contains($":{connectionId}") && connStr.StartsWith(username))
                    {
                        await _db.SetRemoveAsync(_instanceConnectionsKey, conn);
                    }
                }

                // Check if user has no more connections
                if (await _db.SetLengthAsync(userKey) == 0)
                {
                    // No more connections, remove from online users set
                    await _db.KeyDeleteAsync(userKey);
                    
                    // Remove from online users using pattern matching
                    var onlineUsers = await _db.SetMembersAsync(OnlineUsersKey);
                    foreach (var user in onlineUsers)
                    {
                        if (user.ToString().StartsWith($"{username}:"))
                        {
                            await _db.SetRemoveAsync(OnlineUsersKey, user);
                        }
                    }
                    
                    isOffline = true;
                    _logger.Debug($"User {username} (ID: {userId}) is now offline (removed connection {connectionId})");
                }
                else
                {
                    _logger.Debug(
                        $"Removed connection {connectionId} for user {username}, but user still has other connections");
                }

                // Update the instance heartbeat
                await UpdateInstanceHeartbeat();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error disconnecting user {username} with connection {connectionId}");
            }

            return isOffline;
        }

        /// <summary>
        /// Gets just the IDs of all currently online users (simple version)
        /// </summary>
        public async Task<List<int>> GetOnlineUserIds()
        {
            try
            {
                var result = new List<int>();
                var onlineUsers = await _db.SetMembersAsync(OnlineUsersKey);
        
                foreach (var userEntry in onlineUsers)
                {
                    string userEntryStr = userEntry.ToString();
                    string[] parts = userEntryStr.Split(':');
            
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int userId))
                    {
                        result.Add(userId);
                    }
                }
        
                return result.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting online user IDs");
                return new List<int>();
            }
        }
        
        /// <summary>
        /// Gets detailed information about all currently online users
        /// </summary>
        public async Task<List<OnlineUserInfo>> GetOnlineUsersDetailed()
        {
            try
            {
                var result = new List<OnlineUserInfo>();
                var onlineUsers = await _db.SetMembersAsync(OnlineUsersKey);
        
                foreach (var userEntry in onlineUsers)
                {
                    string userEntryStr = userEntry.ToString();
                    string[] parts = userEntryStr.Split(':');
            
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int userId))
                    {
                        result.Add(new OnlineUserInfo { 
                            Username = parts[0], 
                            UserId = userId 
                        });
                    }
                    else
                    {
                        _logger.Warning($"Invalid user entry format in online users set: {userEntryStr}");
                    }
                }
        
                return result.OrderBy(x => x.Username).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting detailed online users");
                return new List<OnlineUserInfo>();
            }
        }

        /// <summary>
        /// Gets all connection IDs for a specific user
        /// </summary>
        public async Task<List<string>> GetConnectionsForUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _logger.Warning("Attempted to get connections for null or empty username");
                return new List<string>();
            }

            try
            {
                var userKey = $"{UserConnectionPrefix}{username}";
                var connections = await _db.SetMembersAsync(userKey);

                return connections.Select(c => c.ToString()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error getting connections for user {username}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Cleans up any stale connections from a dead instance
        /// </summary>
        private async Task CleanupInstanceConnections(string instanceId)
        {
            var instanceConnectionsKey = $"{InstanceConnectionPrefix}{instanceId}";

            try
            {
                _logger.Debug($"Looking for connections in this instance: {instanceConnectionsKey}");
                var connections = await _db.SetMembersAsync(instanceConnectionsKey);
                _logger.Debug($"Found {connections.Length} connections for this instance: {instanceId}");

                foreach (var connection in connections)
                {
                    var connectionInfo = connection.ToString();
                    var parts = connectionInfo.Split(':');

                    if (parts.Length >= 3)
                    {
                        var username = parts[0];
                        var connectionId = parts[2];

                        _logger.Debug(
                            $"Cleaning up connection {connectionId} for user {username} from instance {instanceId}");

                        // Remove this connection for the user
                        var isOffline = await UserDisconnected(username, connectionId);
                        _logger.Debug(
                            $"Connection {connectionId} for user {username} cleaned up. User is now {(isOffline ? "offline" : "still online with other connections")}");
                    }
                    else
                    {
                        _logger.Warning($"Invalid connection format: {connectionInfo}");
                    }
                }

                // Finally, remove the instance connections key
                _logger.Debug($"Removing instance connections key {instanceConnectionsKey}");
                var deleted = await _db.KeyDeleteAsync(instanceConnectionsKey);
                _logger.Debug($"Instance connections key deleted: {deleted}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error cleaning up connections for instance {instanceId}");
            }
        }

        /// <summary>
        /// Cleans up connections from dead instances
        /// </summary>
        public async Task CleanupDeadConnections()
        {
            _logger.Information("Starting cleanup of dead connections from previous instances");
            try
            {
                // Get all app instances
                var instances = await _db.SetMembersAsync(AppInstancesSetKey);
                _logger.Debug($"Found {instances.Length} instances in Redis");

                foreach (var instance in instances)
                {
                    var instanceId = instance.ToString();
                    if (instanceId == _instanceId)
                    {
                        _logger.Debug($"Skipping current instance {instanceId}");
                        continue; // Skip current instance
                    }

                    var instanceKey = $"{AppInstanceKeyPrefix}{instanceId}";
                    _logger.Debug($"Checking instance {instanceId} with key {instanceKey}");

                    // Check if the instance key still exists (hasn't expired)
                    var exists = await _db.KeyExistsAsync(instanceKey);
                    if (!exists)
                    {
                        _logger.Warning($"Found dead instance {instanceId}, cleaning up its connections");
                        await CleanupInstanceConnections(instanceId);
                        _logger.Warning($"Removing dead instance {instanceId} from instances set");
                        await _db.SetRemoveAsync(AppInstancesSetKey, instanceId);
                        _logger.Warning($"Successfully removed dead instance {instanceId}");
                    }
                    else
                    {
                        _logger.Debug($"Instance {instanceId} is still active (key exists)");
                    }
                }

                // Re-register this instance
                _logger.Warning($"Re-registering current instance {_instanceId}");
                await RegisterInstance();

                _logger.Information("Dead connection cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during dead connection cleanup");
            }
        }

        /// <summary>
        /// Validates all connections to ensure they're still active
        /// </summary>
        public async Task<int> ValidateConnections(Func<string, Task> sendKeepAlivePing)
        {
            var removedCount = 0;

            try
            {
                // Get all online users
                var onlineUsers = await GetOnlineUsersDetailed();

                foreach (var userInfo in onlineUsers)
                {
                    // Get all connections for this user
                    var connections = await GetConnectionsForUser(userInfo.Username);

                    foreach (var connectionId in connections)
                    {
                        // Create a TaskCompletionSource for this validation
                        var tcs = new TaskCompletionSource<bool>();
                        _pendingValidations[connectionId] = tcs;

                        try
                        {
                            // Send the keep-alive ping
                            await sendKeepAlivePing(connectionId);

                            // Wait for the response with a timeout
                            var isValid = await Task.WhenAny(
                                tcs.Task,
                                Task.Delay(_connectionValidationTimeout)
                            ) == tcs.Task && await tcs.Task;

                            if (!isValid)
                            {
                                _logger.Warning(
                                    $"Connection {connectionId} for user {userInfo.Username} (ID: {userInfo.UserId}) did not respond to keep-alive within timeout");

                                // Remove the connection
                                await UserDisconnected(userInfo.Username, connectionId);
                                removedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, $"Error validating connection {connectionId} for user {userInfo.Username}");

                            // Remove the invalid connection
                            await UserDisconnected(userInfo.Username, connectionId);
                            removedCount++;
                        }
                        finally
                        {
                            // Clean up the pending validation if it's still there
                            _pendingValidations.TryRemove(connectionId, out _);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating connections");
            }

            return removedCount;
        }

        public void HandleKeepAliveResponse(string connectionId)
        {
            // If this connection has a pending validation, complete it successfully
            if (_pendingValidations.TryRemove(connectionId, out var tcs))
            {
                tcs.TrySetResult(true);
            }
        }
    }
}