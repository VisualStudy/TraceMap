using TraceMap.Models;

namespace TraceMap.Services;

public interface ITracePlaceService
{
    Task<List<TracePlace>> GetAllAsync();
    Task<List<TracePlace>> GetSharedAsync();
    Task<TracePlace?> GetByIdAsync(int id);
    Task<TracePlace> AddAsync(TracePlace place);
    Task<bool> UpdateAsync(TracePlace place);
    Task<bool> DeleteAsync(int id);
    Task<bool> MarkVisitedAsync(int id);
    Task<bool> AddVisitAsync(int id);
    Task<bool> RemoveVisitAsync(int id);
}
