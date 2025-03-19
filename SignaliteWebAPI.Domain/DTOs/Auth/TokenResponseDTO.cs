namespace SignaliteWebAPI.Domain.DTOs.Auth;

public class TokenResponseDTO
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}