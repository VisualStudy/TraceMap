using Microsoft.EntityFrameworkCore;
using TraceMap.Data;
using TraceMap.Models;

namespace TraceMap.Services;

public class TracePlaceService : ITracePlaceService
{
    private readonly ApplicationDbContext _db;

    public TracePlaceService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<TracePlace>> GetAllAsync(string? viewerUserId = null)
    {
        if (string.IsNullOrWhiteSpace(viewerUserId))
        {
            return [];
        }

        var places = await _db.Places
            .Where(p => p.UserId == viewerUserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        await ApplySocialCountsAsync(places);
        ApplyViewerPermissions(places, viewerUserId);
        return places;
    }

    public async Task<List<TracePlace>> GetSharedAsync(string? viewerUserId = null)
    {
        var places = await _db.Places.Where(p => p.IsShared).OrderByDescending(p => p.CreatedAt).ToListAsync();
        await ApplySocialCountsAsync(places);
        ApplyViewerPermissions(places, viewerUserId);
        return places;
    }

    public async Task<TracePlace?> GetByIdAsync(int id, string? viewerUserId = null)
    {
        var place = await _db.Places.FirstOrDefaultAsync(p =>
            p.Id == id &&
            (p.IsShared || (!string.IsNullOrWhiteSpace(viewerUserId) && p.UserId == viewerUserId)));

        if (place is not null)
        {
            await ApplySocialCountsAsync(new List<TracePlace> { place });
            ApplyViewerPermissions(new List<TracePlace> { place }, viewerUserId);
        }

        return place;
    }


    public async Task<bool> CanViewAsync(int id, string? viewerUserId = null)
    {
        return await _db.Places.AnyAsync(p =>
            p.Id == id &&
            (p.IsShared || (!string.IsNullOrWhiteSpace(viewerUserId) && p.UserId == viewerUserId)));
    }

    public async Task<bool> CanModifyAsync(int id, string? viewerUserId = null)
    {
        return await _db.Places.AnyAsync(p =>
            p.Id == id &&
            !string.IsNullOrWhiteSpace(viewerUserId) &&
            p.UserId == viewerUserId);
    }

    public async Task<TracePlace> AddAsync(TracePlace place)
    {
        if (place.IsVisited && place.VisitCount <= 0)
        {
            place.VisitCount = 1;
        }

        if (!place.IsVisited)
        {
            place.VisitCount = 0;
        }

        place.CreatedAt = DateTime.UtcNow;
        place.UpdatedAt = DateTime.UtcNow;
        _db.Places.Add(place);
        await _db.SaveChangesAsync();
        return place;
    }

    public async Task<PlaceWriteResult> UpdateAsync(TracePlace place, string userId)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == place.Id);
        if (existing is null) return PlaceWriteResult.NotFound;
        if (!IsOwner(existing, userId)) return PlaceWriteResult.Forbidden;

        existing.Name = place.Name;
        existing.Category = place.Category;
        existing.Description = place.Description;
        existing.RecommendedActivities = place.RecommendedActivities;
        existing.IsVisited = place.IsVisited;
        existing.VisitCount = place.IsVisited ? Math.Max(place.VisitCount, 1) : 0;
        existing.Latitude = place.Latitude;
        existing.Longitude = place.Longitude;
        existing.IsShared = place.IsShared;
        existing.SharedDescription = place.SharedDescription;
        existing.PhotoUrl = place.PhotoUrl;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return PlaceWriteResult.Success;
    }

    public async Task<PlaceWriteResult> DeleteAsync(int id, string userId)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return PlaceWriteResult.NotFound;
        if (!IsOwner(existing, userId)) return PlaceWriteResult.Forbidden;

        _db.Places.Remove(existing);
        await _db.SaveChangesAsync();
        return PlaceWriteResult.Success;
    }

    public async Task<PlaceWriteResult> MarkVisitedAsync(int id, string userId)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return PlaceWriteResult.NotFound;
        if (!IsOwner(existing, userId)) return PlaceWriteResult.Forbidden;

        existing.IsVisited = true;
        existing.VisitCount = Math.Max(existing.VisitCount, 1);
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return PlaceWriteResult.Success;
    }

    public async Task<PlaceWriteResult> AddVisitAsync(int id, string userId)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return PlaceWriteResult.NotFound;
        if (!IsOwner(existing, userId)) return PlaceWriteResult.Forbidden;

        existing.IsVisited = true;
        existing.VisitCount += 1;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return PlaceWriteResult.Success;
    }

    public async Task<PlaceWriteResult> RemoveVisitAsync(int id, string userId)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return PlaceWriteResult.NotFound;
        if (!IsOwner(existing, userId)) return PlaceWriteResult.Forbidden;

        existing.VisitCount = Math.Max(0, existing.VisitCount - 1);
        existing.IsVisited = existing.VisitCount > 0;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return PlaceWriteResult.Success;
    }

    private async Task ApplySocialCountsAsync(IReadOnlyList<TracePlace> places)
    {
        if (places.Count == 0) return;

        var placeIds = places.Select(place => place.Id).ToList();

        var likeCounts = await _db.PlaceLikes
            .Where(like => placeIds.Contains(like.TracePlaceId))
            .GroupBy(like => like.TracePlaceId)
            .Select(group => new { PlaceId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.PlaceId, item => item.Count);

        var commentCounts = await _db.PlaceComments
            .Where(comment => placeIds.Contains(comment.TracePlaceId))
            .GroupBy(comment => comment.TracePlaceId)
            .Select(group => new { PlaceId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.PlaceId, item => item.Count);

        var photoCounts = await _db.PlacePhotos
            .Where(photo => placeIds.Contains(photo.TracePlaceId) && !photo.IsDeleted)
            .GroupBy(photo => photo.TracePlaceId)
            .Select(group => new { PlaceId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.PlaceId, item => item.Count);

        foreach (var place in places)
        {
            place.LikeCount = likeCounts.TryGetValue(place.Id, out var likeCount) ? likeCount : 0;
            place.CommentCount = commentCounts.TryGetValue(place.Id, out var commentCount) ? commentCount : 0;
            place.PhotoCount = photoCounts.TryGetValue(place.Id, out var photoCount) ? photoCount : 0;
        }
    }

    private static void ApplyViewerPermissions(IReadOnlyList<TracePlace> places, string? viewerUserId)
    {
        foreach (var place in places)
        {
            place.CanModify = IsOwner(place, viewerUserId);
        }
    }

    private static bool IsOwner(TracePlace place, string? userId)
    {
        return !string.IsNullOrWhiteSpace(userId)
            && !string.IsNullOrWhiteSpace(place.UserId)
            && string.Equals(place.UserId, userId, StringComparison.Ordinal);
    }
}
