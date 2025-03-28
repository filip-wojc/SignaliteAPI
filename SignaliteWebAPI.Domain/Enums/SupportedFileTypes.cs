using System.Collections.Immutable;

namespace SignaliteWebAPI.Domain.Enums;

public class SupportedFileTypes
{
    public const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    
    public static ImmutableDictionary<FileType, string[]> MimeTypes = new Dictionary<FileType, string[]>
    {
        { FileType.Image, ["image/jpeg", "image/png", "image/gif","image/webp","image/svg+xml","image/bmp","image/avif"] },
        { FileType.Video, ["video/mp4", "video/webm"] },
        { FileType.Audio, ["audio/mpeg", "audio/wav"] },
        { FileType.Other, ["application/pdf", "application/msword", "application/zip",
                "application/x-zip-compressed", "application/vnd.rar", "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" ] }
    }.ToImmutableDictionary();
    
    
    public static FileType GetFileTypeFromMimeType(string mimeType)
    {
        foreach (var kvp in MimeTypes.Where(kvp => kvp.Value.Contains(mimeType)))
        {
            return kvp.Key;
        }

        return FileType.Unsupported;
    }
    
    public static bool IsSupportedMimeType(string mimeType)
    {
        return MimeTypes.Values.Any(types => types.Contains(mimeType));
    }
    
    
}