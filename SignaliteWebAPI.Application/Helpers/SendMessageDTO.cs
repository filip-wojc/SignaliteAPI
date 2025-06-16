using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Application.Helpers;

public class SendMessageDTO
{
    public string Content { get; set; }
    public int GroupId { get; set; }
    public IFormFile File { get; set; }
}