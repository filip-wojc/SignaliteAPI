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

        public ConnectionCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ConnectionCleanupService> logger,
            TimeSpan? cleanupInterval = null,
            TimeSpan? heartbeatInterval = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(15);
            _heartbeatInterval = heartbeatInterval ?? TimeSpan.FromMinutes(5);
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
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Connection cleanup cycle starting");

                try
                {
                    await CleanupStaleConnections(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during connection cleanup");
                }

                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when shutting down
                    break;
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
            using var scope = _serviceProvider.CreateScope();
            var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PresenceHub>>();
            
            // First, clean up any dead instances
            await presenceTracker.CleanupDeadConnections();
            
            // Next, validate existing connections using ping mechanism
            var removedCount = await presenceTracker.ValidateConnections(async connectionId =>
            {
                try
                {
                    // Create a cancellation token that times out after 5 seconds
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, stoppingToken);
                    
                    // Try to ping the connection to see if it's still alive
                    await hubContext.Clients.Client(connectionId).SendAsync(
                        "KeepAlive", 
                        DateTime.UtcNow, 
                        linkedCts.Token);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Connection {ConnectionId} appears to be dead. Marking for cleanup.", connectionId);
                    return false;
                }
            });
            
            _logger.LogInformation($"Connection cleanup completed. Removed {removedCount} stale connections");
        }
    }
}