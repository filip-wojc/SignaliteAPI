using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Infrastructure.SignalR;

namespace SignaliteWebAPI.Infrastructure.Services
{
    public class ConnectionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConnectionCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval;
        private readonly TimeSpan _heartbeatInterval;
        private readonly bool _skipInitialCleanup;

        public ConnectionCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ConnectionCleanupService> logger,
            TimeSpan? cleanupInterval = null,
            TimeSpan? heartbeatInterval = null,
            bool skipInitialCleanup = false)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(15); 
            _heartbeatInterval = heartbeatInterval ?? TimeSpan.FromMinutes(5);
            _skipInitialCleanup = skipInitialCleanup;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Connection Cleanup Service is starting. Cleanup interval: {_cleanupInterval}, Heartbeat interval: {_heartbeatInterval}");

            // Two parallel timers:
            // 1. Heartbeat timer - keeps the instance marked as alive in Redis
            // 2. Cleanup timer - performs full validation of connections

            // Start the heartbeat timer
            _ = StartHeartbeatTimer(stoppingToken);

            // Start the main cleanup loop
            // Skip the first immediate cleanup if configured to do so
            if (!_skipInitialCleanup)
            {
                _logger.LogInformation("Running initial connection cleanup");
                try
                {
                    await CleanupStaleConnections(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during initial connection cleanup");
                }
            }
            else
            {
                _logger.LogInformation("Skipping initial connection cleanup as configured");
            }

            // Start the regular cleanup loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                    _logger.LogInformation("Scheduled connection cleanup cycle starting");
                    await CleanupStaleConnections(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during scheduled connection cleanup");
                }
            }

            _logger.LogInformation("Connection Cleanup Service is stopping");
        }

        private async Task StartHeartbeatTimer(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
                    
                    // Update heartbeat
                    await presenceTracker.UpdateInstanceHeartbeat();
                    _logger.LogDebug("Instance heartbeat updated");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating instance heartbeat");
                }

                try
                {
                    await Task.Delay(_heartbeatInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when shutting down
                    break;
                }
            }
        }

        private async Task CleanupStaleConnections(CancellationToken stoppingToken)
        {
            _logger.LogInformation("========== STARTING CONNECTION CLEANUP CYCLE ==========");
            
            using var scope = _serviceProvider.CreateScope();
            var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PresenceHub>>();
            
            // First, clean up any dead instances
            _logger.LogInformation("Phase 1: Cleaning up dead instances");
            await presenceTracker.CleanupDeadConnections();
            
            // Next, validate existing connections using ping mechanism
            _logger.LogInformation("Phase 2: Validating existing connections");
            
            var onlineUsers = await presenceTracker.GetOnlineUsers();
            _logger.LogInformation($"Found {onlineUsers.Length} online users to validate connections for");
            
            var removedCount = await presenceTracker.ValidateConnections(async connectionId =>
            {
                try
                {
                    _logger.LogInformation($"Validating connection {connectionId}...");
                    
                    // Create a cancellation token that times out after 5 seconds
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, stoppingToken);
                    
                    // Try to ping the connection to see if it's still alive
                    _logger.LogInformation($"Sending KeepAlive ping to connection {connectionId}");
                    await hubContext.Clients.Client(connectionId).SendAsync(
                        "KeepAlive", 
                        DateTime.UtcNow, 
                        linkedCts.Token);
                    
                    _logger.LogInformation($"Connection {connectionId} is alive");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Connection {connectionId} appears to be dead. Marking for cleanup.");
                    return false;
                }
            });
            
            _logger.LogInformation($"========== CONNECTION CLEANUP CYCLE COMPLETED ==========");
            _logger.LogWarning($"Cleanup summary: Removed {removedCount} stale connections");
        }
    }
}