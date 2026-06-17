using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using TraceMap.Models;

namespace TraceMap.Services;

public sealed class PlacePhotoStorageOptions
{
    public string ContainerName { get; set; } = "place-photos";
    public string LocalRoot { get; set; } = "place-photos";
    public string? ConnectionString { get; set; }
    public bool EnableBlobUpload { get; set; } = true;
}

public class PlacePhotoStorageService : IPlacePhotoStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".heic", ".heif"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PlacePhotoStorageService> _logger;
    private readonly PlacePhotoStorageOptions _options;

    public PlacePhotoStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IOptions<PlacePhotoStorageOptions> options,
        ILogger<PlacePhotoStorageService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<PlacePhoto> SaveAsync(int placeId, IFormFile file, string? userId, string? userName, CancellationToken cancellationToken = default)
    {
        var extension = NormalizeExtension(Path.GetExtension(file.FileName), file.ContentType);
        var contentType = ResolveContentType(file.ContentType, extension);
        ValidateImage(file, extension, contentType);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var blobName = $"{placeId}/{safeFileName}";
        var localRelativePath = $"{_options.LocalRoot}/{blobName}".Replace('\\', '/');
        var localFullPath = GetLocalFullPath(blobName);

        Directory.CreateDirectory(Path.GetDirectoryName(localFullPath)!);

        await using (var output = File.Create(localFullPath))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        var storageProvider = "Local";
        string? blobETag = null;

        if (_options.EnableBlobUpload)
        {
            try
            {
                var blobClient = await GetBlobClientAsync(blobName, cancellationToken);
                if (blobClient is not null)
                {
                    await using var input = File.OpenRead(localFullPath);
                    var result = await blobClient.UploadAsync(input, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                    }, cancellationToken);

                    storageProvider = "Hybrid";
                    blobETag = result.Value.ETag.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blob upload failed for {BlobName}. The local backup file was saved successfully.", blobName);
            }
        }

        return new PlacePhoto
        {
            TracePlaceId = placeId,
            FileName = safeFileName,
            BlobName = blobName,
            LocalRelativePath = localRelativePath,
            ContentType = contentType,
            Size = file.Length,
            StorageProvider = storageProvider,
            BlobETag = blobETag,
            UserId = userId,
            UserName = userName,
            IsAnonymous = string.IsNullOrWhiteSpace(userId),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<PlacePhoto?> ReplaceAsync(PlacePhoto existing, IFormFile file, CancellationToken cancellationToken = default)
    {
        var extension = NormalizeExtension(Path.GetExtension(file.FileName), file.ContentType);
        var contentType = ResolveContentType(file.ContentType, extension);
        ValidateImage(file, extension, contentType);

        var localFullPath = Path.Combine(_environment.WebRootPath, existing.LocalRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(localFullPath)!);

        await using (var output = File.Create(localFullPath))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        existing.ContentType = contentType;
        existing.Size = file.Length;
        existing.UpdatedAt = DateTime.UtcNow;

        if (_options.EnableBlobUpload)
        {
            try
            {
                var blobClient = await GetBlobClientAsync(existing.BlobName, cancellationToken);
                if (blobClient is not null)
                {
                    await using var input = File.OpenRead(localFullPath);
                    var result = await blobClient.UploadAsync(input, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = existing.ContentType }
                    }, cancellationToken);
                    existing.StorageProvider = "Hybrid";
                    existing.BlobETag = result.Value.ETag.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blob replace failed for {BlobName}. The local backup file was replaced successfully.", existing.BlobName);
            }
        }

        return existing;
    }

    public async Task<PlacePhotoReadResult?> OpenReadAsync(PlacePhoto photo, CancellationToken cancellationToken = default)
    {
        if (_options.EnableBlobUpload)
        {
            try
            {
                var blobClient = await GetBlobClientAsync(photo.BlobName, cancellationToken);
                if (blobClient is not null && await blobClient.ExistsAsync(cancellationToken))
                {
                    var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
                    return new PlacePhotoReadResult(response.Value.Content, photo.ContentType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Blob download failed for {BlobName}. Trying local backup file.", photo.BlobName);
            }
        }

        var localFullPath = Path.Combine(_environment.WebRootPath, photo.LocalRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(localFullPath)) return null;

        return new PlacePhotoReadResult(File.OpenRead(localFullPath), photo.ContentType);
    }

    public async Task DeleteAsync(PlacePhoto photo, CancellationToken cancellationToken = default)
    {
        var localFullPath = Path.Combine(_environment.WebRootPath, photo.LocalRelativePath.Replace('/', Path.DirectorySeparatorChar));
        try
        {
            if (File.Exists(localFullPath)) File.Delete(localFullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local photo delete failed for {LocalPath}.", photo.LocalRelativePath);
        }

        if (!_options.EnableBlobUpload) return;

        try
        {
            var blobClient = await GetBlobClientAsync(photo.BlobName, cancellationToken);
            if (blobClient is not null) await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Blob photo delete failed for {BlobName}.", photo.BlobName);
        }
    }

    private async Task<BlobClient?> GetBlobClientAsync(string blobName, CancellationToken cancellationToken)
    {
        var connectionString = ResolveConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString)) return null;

        var serviceClient = new BlobServiceClient(connectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient.GetBlobClient(blobName);
    }

    private string? ResolveConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(_options.ConnectionString)) return _options.ConnectionString;

        var direct = _configuration.GetConnectionString("AzureBlobStorage")
            ?? _configuration["AzureBlobStorage:ConnectionString"]
            ?? _configuration["AzureStorage:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(direct)) return direct;

        var account = _configuration["AppKeys:AzureStorageAccount"];
        var key = _configuration["AppKeys:AzureStorageAccessKey"];
        var suffix = _configuration["AppKeys:AzureStorageEndpointSuffix"] ?? "core.windows.net";

        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(key)) return null;

        return $"DefaultEndpointsProtocol=https;AccountName={account};AccountKey={key};EndpointSuffix={suffix}";
    }

    private string GetLocalFullPath(string blobName)
    {
        return Path.Combine(_environment.WebRootPath, _options.LocalRoot, blobName.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void ValidateImage(IFormFile file, string extension, string contentType)
    {
        if (file.Length <= 0) throw new InvalidOperationException("빈 파일은 업로드할 수 없습니다.");
        if (file.Length > 10 * 1024 * 1024) throw new InvalidOperationException("사진은 10MB 이하만 업로드할 수 있습니다.");

        if (!AllowedExtensions.Contains(extension)) throw new InvalidOperationException("jpg, png, webp, gif, heic 이미지만 업로드할 수 있습니다.");

        // Android 앱의 Multipart 업로드는 기기/갤러리 앱에 따라 image/jpeg 대신
        // application/octet-stream으로 들어오는 경우가 있어, 확장자가 이미지이면
        // 서버에서 이미지 MIME으로 보정한다. 단, 명백한 비이미지 MIME은 계속 차단한다.
        if (!string.IsNullOrWhiteSpace(file.ContentType)
            && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            && !IsUnknownBinaryContentType(file.ContentType))
        {
            throw new InvalidOperationException("이미지 파일만 업로드할 수 있습니다.");
        }

        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("이미지 파일만 업로드할 수 있습니다.");
        }
    }

    private static bool IsUnknownBinaryContentType(string contentType)
    {
        return contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase)
            || contentType.Equals("binary/octet-stream", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveContentType(string? contentType, string extension)
    {
        if (!string.IsNullOrWhiteSpace(contentType)
            && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) ? "image/jpeg" : contentType;
        }

        return GetContentType(extension);
    }

    private static string NormalizeExtension(string extension, string? contentType)
    {
        if (AllowedExtensions.Contains(extension)) return extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : extension.ToLowerInvariant();
        return contentType?.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/heic" => ".heic",
            "image/heif" => ".heif",
            _ => ".jpg"
        };
    }

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        ".heic" => "image/heic",
        ".heif" => "image/heif",
        _ => "image/jpeg"
    };
}
