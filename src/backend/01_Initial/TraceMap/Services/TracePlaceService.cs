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

    public Task<List<TracePlace>> GetAllAsync() =>
        _db.Places.OrderByDescending(p => p.CreatedAt).ToListAsync();

    public Task<List<TracePlace>> GetSharedAsync() =>
        _db.Places.Where(p => p.IsShared).OrderByDescending(p => p.CreatedAt).ToListAsync();

    public Task<TracePlace?> GetByIdAsync(int id) =>
        _db.Places.FirstOrDefaultAsync(p => p.Id == id);

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
}
