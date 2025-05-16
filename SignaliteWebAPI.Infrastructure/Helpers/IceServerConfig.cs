namespace SignaliteWebAPI.Infrastructure.Helpers;

/// <summary>
/// Configuration for an ICE server (STUN or TURN)
/// </summary>
public class IceServerConfig
{
    public string Urls { get; set; }
    public string Username { get; set; }
    public string Credential { get; set; }
}