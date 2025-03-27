using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SignaliteWebAPI.Infrastructure.Helpers;
using SignaliteWebAPI.Infrastructure.Interfaces;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Infrastructure.Services.Media;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;
    private const int CHUNK_SIZE = 6000000; // 6MB chunks
    private const int VIDEO_SIZE_THRESHOLD = 25000000;

    public MediaService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }
    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                //Transformation = new Transformation()
                //    .Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "Signalite"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<VideoUploadResult> AddVideoAsync(IFormFile file)
    {
        if (file.Length <= 0)
        {
            return new VideoUploadResult 
            { 
                Error = new Error { Message = "Empty file provided" } 
            };
        }

        try
        {
            // For smaller videos, use standard upload
            if (file.Length < VIDEO_SIZE_THRESHOLD)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "Signalite/Videos",
                    // Optional video settings
                    // EagerTransforms = new List<Transformation>
                    // {
                    //     new Transformation().Quality("auto")
                    // }
                };

                return await _cloudinary.UploadAsync(uploadParams);
            }
            // For larger videos, use UploadLargeAsync which handles chunking internally
            else
            {
                return await UploadLargeVideoAsync(file);
            }
        }
        catch (Exception ex)
        {
            return new VideoUploadResult
            {
                Error = new Error { Message = $"Video upload failed: {ex.Message}" }
            };
        }
    }
    
    private async Task<VideoUploadResult> UploadLargeVideoAsync(IFormFile file)
    {
        // Create a temporary file to ensure reliable stream handling for large files
        var tempFilePath = Path.GetTempFileName();
        try
        {
            // Save the uploaded file to a temp file
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // UploadLargeAsync handles chunking internally
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, tempFilePath),
                Folder = "Signalite/Videos",

                // ChunkSize = CHUNK_SIZE, // Default is 20MB
                // Timeout = 600000, // Increase timeout for large uploads (in milliseconds)
                UseFilename = true,
                UniqueFilename = true
            };

            return await _cloudinary.UploadLargeAsync(uploadParams);
        }
        finally
        {
            // Clean up the temp file
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public Task<VideoUploadResult> AddAudioAsync(IFormFile file)
    {
        throw new NotImplementedException();
    }

    public Task<RawUploadResult> AddDocumentAsync(IFormFile file)
    {
        throw new NotImplementedException();
    }

    public Task<DeletionResult> DeleteMediaAsync(string publicId)
    {
        throw new NotImplementedException();
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
