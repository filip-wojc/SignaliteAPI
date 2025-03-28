using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface IMediaService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<VideoUploadResult> AddVideoAsync(IFormFile file);
    Task<VideoUploadResult> AddAudioAsync(IFormFile file);
    Task<DeletionResult> DeleteMediaAsync(string publicId, string mimeType);


}