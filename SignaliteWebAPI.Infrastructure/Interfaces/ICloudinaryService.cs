using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Infrastructure.Interfaces;

public interface ICloudinaryService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}