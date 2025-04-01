using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SignaliteWebAPI.Infrastructure.Helpers;

namespace SignaliteWebAPI.Infrastructure.Interfaces.Services;

public interface IMediaService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<VideoUploadResult> AddVideoAsync(IFormFile file);
    Task<VideoUploadResult> AddAudioAsync(IFormFile file);
    Task<DeletionResult> DeleteMediaAsync(string publicId, string mimeType = "image/jpeg");
    Task<StaticFileResult> AddStaticFile(IFormFile file);
    Task DeleteStaticFile(string url);
}