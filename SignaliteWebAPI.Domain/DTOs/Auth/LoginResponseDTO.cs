namespace SignaliteWebAPI.Domain.DTOs.Auth;

public class LoginResponseDTO
{
    public required int UserId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}