using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private const string OnlineUsersKey = "online-users";
        private const string AppInstancesSetKey = "app:instances";
        private const string AppInstanceKeyPrefix = "app:instance:";
        private const string InstanceConnectionPrefix = "instance:connections:";
        
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
            
            _logger.Information($"Instance {_instanceId} registered");
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
                    _logger.Debug($"User {username} is now online with connection {connectionId}");
                }
                else
                {
                    _logger.Debug($"Additional connection {connectionId} for user {username}");
                }
                
                // Update the instance heartbeat
                await UpdateInstanceHeartbeat();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error connecting user {username} with connection {connectionId}");
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
                    _logger.Debug($"User {username} is now offline (removed connection {connectionId})");
                }
                else
                {
                    _logger.Debug($"Removed connection {connectionId} for user {username}, but user still has other connections");
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
                _logger.Error(ex, "Error getting online users");
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
                // Get all connections for this instance
                _logger.Debug($"Looking for connections in this instance: {instanceConnectionsKey}");
                var connections = await _db.SetMembersAsync(instanceConnectionsKey);
                _logger.Debug($"Found {connections.Length} connections for this instance: {instanceId}");
                
                foreach (var connection in connections)
                {
                    var connectionInfo = connection.ToString();
                    var parts = connectionInfo.Split(':');
                    
                    if (parts.Length == 2)
                    {
                        var username = parts[0];
                        var connectionId = parts[1];
                        
                        _logger.Debug($"Cleaning up connection {connectionId} for user {username} from instance {instanceId}");
                        
                        // Remove this connection for the user
                        var isOffline = await UserDisconnected(username, connectionId);
                        _logger.Debug($"Connection {connectionId} for user {username} cleaned up. User is now {(isOffline ? "offline" : "still online with other connections")}");
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
                        
                        // Clean up connections for this dead instance
                        await CleanupInstanceConnections(instanceId);
                        
                        // Remove instance from instances set
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
                            _logger.Warning($"Removing invalid connection {connectionId} for user {username}");
                            
                            // Remove the invalid connection
                            await UserDisconnected(username, connectionId);
                            removedCount++;
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
    }
}