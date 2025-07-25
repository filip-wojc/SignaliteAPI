using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SignaliteWebAPI.Domain.Enums;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Helpers;
using SignaliteWebAPI.Infrastructure.Interfaces.Services;

namespace SignaliteWebAPI.Infrastructure.Services;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;
    private readonly AttachmentPath _staticFilesPath;
    
    private const int CHUNK_SIZE = 6000000; // 6MB chunks
    private const int VIDEO_SIZE_THRESHOLD = 25000000;
    
    
    public MediaService(IOptions<CloudinarySettings> cloudinaryConfig, AttachmentPath staticFilesPath)
    {
        var account = new Account(cloudinaryConfig.Value.CloudName, cloudinaryConfig.Value.ApiKey, cloudinaryConfig.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _staticFilesPath = staticFilesPath;
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new CloudinaryException("Empty file provided");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "Signalite/Photos"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
            {
                throw new CloudinaryException($"Failed to upload photo: {uploadResult.Error.Message}");
            }
            
            return uploadResult;
        }
        catch (Exception ex) when (ex is not CloudinaryException)
        {
            // only wrap non-cloudinaryExceptions
            throw new MediaServiceException($"Photo upload failed: {ex.Message}");
        }
    }


    public async Task<VideoUploadResult> AddVideoAsync(IFormFile file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new CloudinaryException("Empty file provided");
        }

        try
        {
            VideoUploadResult uploadResult;

            // For smaller videos, use standard upload
            if (file.Length < VIDEO_SIZE_THRESHOLD)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "Signalite/Videos"
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            // For larger videos, use UploadLargeAsync which handles chunking internally
            else
            {
                uploadResult = await UploadLargeVideoAsync(file);
            }

            if (uploadResult.Error != null)
            {
                throw new CloudinaryException($"Failed to upload video: {uploadResult.Error.Message}");
            }

            return uploadResult;
        }
        catch (Exception ex) when (!(ex is CloudinaryException))
        {
            throw new MediaServiceException($"Video upload failed: {ex.Message}");
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
                UseFilename = true,
                UniqueFilename = true
            };

            return await _cloudinary.UploadLargeAsync(uploadParams);
        }
        catch (Exception ex)
        {
            throw new MediaServiceException($"Failed to upload large video: {ex.Message}");
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

    // adds audio to the "Singalite/Audio" folder, uses the same pipeline as video upload
    public async Task<VideoUploadResult> AddAudioAsync(IFormFile file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new CloudinaryException("Empty file provided");
        }
        
        try
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "Signalite/Audio"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new CloudinaryException($"Failed to upload audio: {uploadResult.Error.Message}");
            }
            
            return uploadResult;
        }
        catch (Exception ex) when (ex is not CloudinaryException)
        {
            throw new MediaServiceException($"Audio upload failed: {ex.Message}");
        }
    }

    public async Task<DeletionResult> DeleteMediaAsync(string publicId, string mimeType = "image/jpeg") // default image format if no argument
    {
        if (string.IsNullOrEmpty(publicId))
        {
            throw new CloudinaryException("Public ID cannot be null or empty");
        }
        
        var fileType = SupportedFileTypes.GetFileTypeFromMimeType(mimeType);

        var resourceType = fileType switch
        {
            FileType.Image => ResourceType.Image,
            FileType.Video or FileType.Audio => ResourceType.Video,
            FileType.Other => throw new NotImplementedException("Other file types not implemented"),
            FileType.Unsupported => throw new CloudinaryException("Unsupported file type"),
            _ => throw new MediaServiceException("Error handling file type in MediaService")
        };
        
        var deleteParams = new DeletionParams(publicId){ResourceType = resourceType};

        try
        {
            return await _cloudinary.DestroyAsync(deleteParams);
        }
        catch (Exception ex)
        {
            throw new MediaServiceException($"Failed to delete media: {ex.Message}");
        }
    }

    public async Task<StaticFileResult> AddStaticFile(IFormFile file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new MediaServiceException("Empty file provided");
        }

        try
        {
            var fileName = $"{file.FileName.Replace(" ", "_")}";
            int lastDotIndex = fileName.LastIndexOf('.');
            fileName = fileName.Insert(lastDotIndex, $"_{Guid.NewGuid().ToString("N")}");
            
            var fullPath = Path.Combine(_staticFilesPath.AttachmentsDirectory, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileSize = (file.Length / (1024 * 1024)).ToString();
            var publicUrl = $"{_staticFilesPath.RequestUrl.TrimEnd('/')}/{fileName}";

            return new StaticFileResult
            {
                FileName = fileName,
                FileSize = fileSize,
                PublicId = Guid.NewGuid().ToString("N"),
                Url = publicUrl,
                Type = file.ContentType
            };
        }
        catch (Exception ex)
        {
            throw new MediaServiceException($"Failed to add static file: {ex.Message}");
        }
    }

    public void DeleteStaticFile(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new MediaServiceException("URL cannot be null or empty");
        }

        try
        {
            string fileName = Path.GetFileName(url);
            
            string fullPath = Path.Combine(_staticFilesPath.AttachmentsDirectory, fileName);
            
            if (!File.Exists(fullPath))
            {
                throw new MediaServiceException($"File not found: {fileName}");
            }
            
            File.Delete(fullPath);
        }
        catch (Exception ex) when (ex is not MediaServiceException)
        {
            throw new MediaServiceException($"Failed to delete static file: {ex.Message}");
        }
    }

}