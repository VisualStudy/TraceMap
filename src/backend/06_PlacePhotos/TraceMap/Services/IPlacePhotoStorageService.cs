using TraceMap.Models;

namespace TraceMap.Services;

public interface IPlacePhotoStorageService
{
    Task<PlacePhoto> SaveAsync(int placeId, IFormFile file, string? userId, string? userName, CancellationToken cancellationToken = default);
    Task<PlacePhoto?> ReplaceAsync(PlacePhoto existing, IFormFile file, CancellationToken cancellationToken = default);
    Task<PlacePhotoReadResult?> OpenReadAsync(PlacePhoto photo, CancellationToken cancellationToken = default);
    Task DeleteAsync(PlacePhoto photo, CancellationToken cancellationToken = default);
}

public sealed record PlacePhotoReadResult(Stream Content, string ContentType);
