using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignaliteWebAPI.Infrastructure.SignalR
{
    public class PresenceTracker
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<PresenceTracker> _logger;
        
        // Key prefixes and constants
        private const string UserConnectionPrefix = "user:";
        private const string OnlineUsersKey = "online-users";
        private const string AppInstancesSetKey = "app:instances";
        private const string AppInstanceKeyPrefix = "app:instance:";
        private const string InstanceConnectionPrefix = "instance:connections:";
        
        // Timeouts and expiration times
        private static readonly TimeSpan InstanceHeartbeatInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan InstanceExpirationTime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan KeyExpirationTime = TimeSpan.FromDays(1); // Failsafe
        
        private readonly string _instanceId;
        private readonly string _instanceKey;
        private readonly string _instanceConnectionsKey;
        
        public PresenceTracker(IConnectionMultiplexer redis, ILogger<PresenceTracker> logger)
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
            
            _logger.LogInformation($"Instance {_instanceId} registered");
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
                _logger.LogInformation($"Unregistering instance {_instanceId}");
                
                // Clean up all connections for this instance
                await CleanupInstanceConnections(_instanceId);
                
                // Remove the instance from the instances set
                await _db.SetRemoveAsync(AppInstancesSetKey, _instanceId);
                
                // Delete the instance key
                await _db.KeyDeleteAsync(_instanceKey);
                
                _logger.LogInformation($"Instance {_instanceId} unregistered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unregistering instance {_instanceId}");
            }
        }
        
        /// <summary>
        /// Marks a user as connected with the specified connection ID
        /// </summary>
        public async Task<bool> UserConnected(string username, string connectionId)
        {
            var isOnline = false;
            var userKey = $"{UserConnectionPrefix}{username}";
            var connectionInfo = $"{username}:{connectionId}";
            
            try
            {
                // Add connection ID to user's set with expiration
                await _db.SetAddAsync(userKey, connectionId);
                await _db.KeyExpireAsync(userKey, KeyExpirationTime);
                
                // Add connection to this instance's set
                await _db.SetAddAsync(_instanceConnectionsKey, connectionInfo);
                
                // Check if this is the first connection for the user
                if (await _db.SetLengthAsync(userKey) == 1)
                {
                    // First connection, add to online users set
                    await _db.SetAddAsync(OnlineUsersKey, username);
                    isOnline = true;
                    _logger.LogDebug($"User {username} is now online with connection {connectionId}");
                }
                else
                {
                    _logger.LogDebug($"Additional connection {connectionId} for user {username}");
                }
                
                // Update the instance heartbeat
                await UpdateInstanceHeartbeat();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error connecting user {username} with connection {connectionId}");
            }
            
            return isOnline;
        }

        /// <summary>
        /// Marks a user as disconnected for the specified connection ID
        /// </summary>
        public async Task<bool> UserDisconnected(string username, string connectionId)
        {
            var isOffline = false;
            var userKey = $"{UserConnectionPrefix}{username}";
            var connectionInfo = $"{username}:{connectionId}";
            
            try
            {
                // Remove connection ID from user's set
                await _db.SetRemoveAsync(userKey, connectionId);
                
                // Remove connection from instance connections
                await _db.SetRemoveAsync(_instanceConnectionsKey, connectionInfo);
                
                // Check if user has no more connections
                if (await _db.SetLengthAsync(userKey) == 0)
                {
                    // No more connections, remove from online users set
                    await _db.KeyDeleteAsync(userKey);
                    await _db.SetRemoveAsync(OnlineUsersKey, username);
                    isOffline = true;
                    _logger.LogDebug($"User {username} is now offline (removed connection {connectionId})");
                }
                else
                {
                    _logger.LogDebug($"Removed connection {connectionId} for user {username}, but user still has other connections");
                }
                
                // Update the instance heartbeat
                await UpdateInstanceHeartbeat();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disconnecting user {username} with connection {connectionId}");
            }
            
            return isOffline;
        }

        /// <summary>
        /// Gets all currently online users
        /// </summary>
        public async Task<string[]> GetOnlineUsers()
        {
            try
            {
                var onlineUsers = await _db.SetMembersAsync(OnlineUsersKey);
                return onlineUsers.Select(m => m.ToString()).OrderBy(x => x).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return Array.Empty<string>();
            }
        }
        
        /// <summary>
        /// Gets all connection IDs for a specific user
        /// </summary>
        public async Task<List<string>> GetConnectionsForUser(string username)
        {
            try
            {
                var userKey = $"{UserConnectionPrefix}{username}";
                var connections = await _db.SetMembersAsync(userKey);
                
                return connections.Select(c => c.ToString()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting connections for user {username}");
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
                // Get all connections for this instance
                var connections = await _db.SetMembersAsync(instanceConnectionsKey);
                
                foreach (var connection in connections)
                {
                    var connectionInfo = connection.ToString();
                    var parts = connectionInfo.Split(':');
                    
                    if (parts.Length == 2)
                    {
                        var username = parts[0];
                        var connectionId = parts[1];
                        
                        _logger.LogInformation($"Cleaning up connection {connectionId} for user {username} from instance {instanceId}");
                        
                        // Remove this connection for the user
                        await UserDisconnected(username, connectionId);
                    }
                }
                
                // Finally, remove the instance connections key
                await _db.KeyDeleteAsync(instanceConnectionsKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cleaning up connections for instance {instanceId}");
            }
        }
        
        /// <summary>
        /// Cleans up connections from dead instances
        /// </summary>
        public async Task CleanupDeadConnections()
        {
            _logger.LogInformation("Starting cleanup of dead connections from previous instances");
            
            try
            {
                // Get all app instances
                var instances = await _db.SetMembersAsync(AppInstancesSetKey);
                
                foreach (var instance in instances)
                {
                    var instanceId = instance.ToString();
                    if (instanceId == _instanceId) continue; // Skip current instance
                    
                    var instanceKey = $"{AppInstanceKeyPrefix}{instanceId}";
                    
                    // Check if the instance key still exists (hasn't expired)
                    var exists = await _db.KeyExistsAsync(instanceKey);
                    if (!exists)
                    {
                        _logger.LogInformation($"Found dead instance {instanceId}, cleaning up its connections");
                        
                        // Clean up connections for this dead instance
                        await CleanupInstanceConnections(instanceId);
                        
                        // Remove instance from instances set
                        await _db.SetRemoveAsync(AppInstancesSetKey, instanceId);
                    }
                }
                
                // Also clean up this instance's connections on startup (in case of crash and restart)
                await CleanupInstanceConnections(_instanceId);
                
                // Re-register this instance
                await RegisterInstance();
                
                _logger.LogInformation("Dead connection cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during dead connection cleanup");
            }
        }
        
        /// <summary>
        /// Validates all connections to ensure they're still active
        /// </summary>
        public async Task<int> ValidateConnections(Func<string, Task<bool>> connectionValidator)
        {
            var removedCount = 0;
            
            try
            {
                // Get all online users
                var onlineUsers = await GetOnlineUsers();
                
                foreach (var username in onlineUsers)
                {
                    // Get all connections for this user
                    var connections = await GetConnectionsForUser(username);
                    
                    foreach (var connectionId in connections)
                    {
                        // Validate the connection
                        var isValid = await connectionValidator(connectionId);
                        
                        if (!isValid)
                        {
                            _logger.LogInformation($"Removing invalid connection {connectionId} for user {username}");
                            
                            // Remove the invalid connection
                            await UserDisconnected(username, connectionId);
                            removedCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating connections");
            }
            
            return removedCount;
        }
    }
}