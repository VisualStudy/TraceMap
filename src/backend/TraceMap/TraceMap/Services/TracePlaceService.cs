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

    public async Task<List<TracePlace>> GetAllAsync()
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
        return places;
    }

    public async Task<List<TracePlace>> GetSharedAsync()
    {
        var places = await _db.Places.Where(p => p.IsShared).OrderByDescending(p => p.CreatedAt).ToListAsync();
        await ApplySocialCountsAsync(places);
        return places;
    }

    public async Task<TracePlace?> GetByIdAsync(int id)
    {
        var place = await _db.Places.FirstOrDefaultAsync(p =>
            p.Id == id &&
            (p.IsShared || (!string.IsNullOrWhiteSpace(viewerUserId) && p.UserId == viewerUserId)));

        if (place is not null)
        {
            await ApplySocialCountsAsync(new List<TracePlace> { place });
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

    public async Task<bool> UpdateAsync(TracePlace place)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == place.Id);
        if (existing is null) return false;

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
        existing.UserId = place.UserId;
        existing.UserName = place.UserName;
        existing.IsAnonymous = place.IsAnonymous;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Places.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return false;
        _db.Places.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkVisitedAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing is null) return false;
        existing.IsVisited = true;
        existing.VisitCount = Math.Max(existing.VisitCount, 1);
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddVisitAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing is null) return false;
        existing.IsVisited = true;
        existing.VisitCount += 1;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveVisitAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing is null) return false;
        existing.VisitCount = Math.Max(0, existing.VisitCount - 1);
        existing.IsVisited = existing.VisitCount > 0;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
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

}
