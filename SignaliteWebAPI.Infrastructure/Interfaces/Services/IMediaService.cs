using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface IMediaService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}