using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using SignaliteWebAPI.Infrastructure.Helpers;
using ILogger = Serilog.ILogger;

namespace SignaliteWebAPI.Infrastructure.Services
{
    public class WebRtcConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public WebRtcConfigService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets the configured ICE servers for WebRTC
        /// </summary>
        public List<IceServerConfig> GetIceServers()
        {
            try
            {
                var iceServers = new List<IceServerConfig>();
                
                // Get ICE servers from configuration
                var iceServerSection = _configuration.GetSection("WebRTC:IceServers");
                
                if (iceServerSection.Exists())
                {
                    foreach (var server in iceServerSection.GetChildren())
                    {
                        var urls = server["urls"];
                        var username = server["username"];
                        var credential = server["credential"];
                        
                        if (!string.IsNullOrEmpty(urls))
                        {
                            iceServers.Add(new IceServerConfig
                            {
                                Urls = urls,
                                Username = username,
                                Credential = credential
                            });
                        }
                    }
                }
                
                // If no ICE servers are configured, add some default public STUN servers
                if (iceServers.Count == 0)
                {
                    _logger.Error("No ICE servers configured, using default public STUN servers");
                    iceServers.Add(new IceServerConfig { Urls = "stun:stun.l.google.com:19302" });
                    iceServers.Add(new IceServerConfig { Urls = "stun:stun1.l.google.com:19302" });
                }

                return iceServers;
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error getting ICE servers configuration");
                
                // Return default servers as fallback
                return new List<IceServerConfig>
                {
                    new IceServerConfig { Urls = "stun:stun.l.google.com:19302" },
                    new IceServerConfig { Urls = "stun:stun1.l.google.com:19302" }
                };
            }
        }
    }

    
}