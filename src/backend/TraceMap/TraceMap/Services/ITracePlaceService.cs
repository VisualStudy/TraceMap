using TraceMap.Models;

namespace TraceMap.Services;

public enum PlaceWriteResult
{
    Success,
    NotFound,
    Forbidden
}

public interface ITracePlaceService
{
    Task<List<TracePlace>> GetAllAsync(string? viewerUserId = null);
    Task<List<TracePlace>> GetSharedAsync(string? viewerUserId = null);
    Task<TracePlace?> GetByIdAsync(int id, string? viewerUserId = null);
    Task<TracePlace> AddAsync(TracePlace place);
    Task<PlaceWriteResult> UpdateAsync(TracePlace place, string userId);
    Task<PlaceWriteResult> DeleteAsync(int id, string userId);
    Task<PlaceWriteResult> MarkVisitedAsync(int id, string userId);
    Task<PlaceWriteResult> AddVisitAsync(int id, string userId);
    Task<PlaceWriteResult> RemoveVisitAsync(int id, string userId);
}
