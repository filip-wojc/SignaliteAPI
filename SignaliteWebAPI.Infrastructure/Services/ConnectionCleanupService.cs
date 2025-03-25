using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignaliteWebAPI.Infrastructure.SignalR;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Infrastructure.Services
{
    public class ConnectionCleanupService : BackgroundService
    {
        
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        // Two parallel timers:
        // 1. Heartbeat timer - keeps the instance marked as alive in Redis
        // 2. Cleanup timer - performs full validation of connections
        private readonly TimeSpan _cleanupInterval;
        private readonly TimeSpan _heartbeatInterval;
        private readonly bool _skipInitialCleanup;

        
        public ConnectionCleanupService(
            IServiceProvider serviceProvider,
            ILogger logger,
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
            _logger.Information($"ConnectionCleanupService starting. Cleanup interval: {_cleanupInterval}, Heartbeat interval: {_heartbeatInterval}");
            // Start the heartbeat timer
            _ = StartHeartbeatTimer(stoppingToken);

            // Start the main cleanup loop
            // Skip the first immediate cleanup if configured to do so
            if (!_skipInitialCleanup)
            {
                _logger.Information("Running initial connection cleanup");
                try
                {
                    await CleanupStaleConnections(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error occurred during initial connection cleanup");
                }
            }
            else
            {
                _logger.Information("Skipping initial connection cleanup as configured");
            }

            // Start the regular cleanup loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                    _logger.Information("Scheduled connection cleanup cycle starting");
                    await CleanupStaleConnections(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error occurred during scheduled connection cleanup");
                }
            }

            _logger.Information("Connection Cleanup Service is stopping");
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
                    _logger.Debug("Instance heartbeat updated");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error updating instance heartbeat");
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
            _logger.Information("========== STARTING CONNECTION CLEANUP CYCLE ==========");
        
            using var scope = _serviceProvider.CreateScope();
            var presenceTracker = scope.ServiceProvider.GetRequiredService<PresenceTracker>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PresenceHub>>();
        
            // First, clean up any dead instances
            _logger.Debug("Phase 1: Cleaning up dead instances");
            await presenceTracker.CleanupDeadConnections();
        
            // Next, validate existing connections using ping-response mechanism
            _logger.Debug("Phase 2: Validating existing connections");
        
            var onlineUsers = await presenceTracker.GetOnlineUsersDetailed();
            _logger.Debug($"Found {onlineUsers.Count} online users to validate connections for");
        
            var removedCount = await presenceTracker.ValidateConnections(async connectionId =>
            {
                _logger.Debug($"Sending KeepAlive ping to connection {connectionId}");
            
                // Send the ping with the current timestamp
                await hubContext.Clients.Client(connectionId).SendAsync(
                    "KeepAlive", 
                    DateTime.UtcNow,
                    stoppingToken);
            });
        
            _logger.Information($"========== CONNECTION CLEANUP CYCLE COMPLETED ==========");
            _logger.Warning($"Cleanup summary: Removed {removedCount} stale connections");
        }
    }
}