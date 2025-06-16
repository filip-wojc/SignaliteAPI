using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Infrastructure.Extensions;
using ILogger = Serilog.ILogger;
namespace SignaliteWebAPI.Infrastructure.SignalR;

public class UsernameUserIdProvider(ILogger logger) : IUserIdProvider
{

        public string GetUserId(HubConnectionContext connection)
        {
            try
            {
                if (connection?.User == null)
                {
                    logger.Warning("Null connection or user in UsernameUserIdProvider");
                    return string.Empty; // Return empty string instead of null
                }
                
                var username = connection.User.GetUsername();

                if (string.IsNullOrEmpty(username))
                {
                    logger.Warning($"Connection {connection.ConnectionId} has no valid username claim");
                    return string.Empty; 
                }

                return username;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in UsernameUserIdProvider.GetUserId");
                return string.Empty;
            }
        }
    }


