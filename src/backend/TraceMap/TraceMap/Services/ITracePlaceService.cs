using TraceMap.Models;

namespace TraceMap.Services;

public interface ITracePlaceService
{
    /// <summary>
    /// 현재 로그인한 사용자가 직접 등록한 개인 장소만 조회합니다.
    /// 비로그인 사용자는 개인 장소 목록을 볼 수 없으므로 빈 목록을 반환합니다.
    /// </summary>
    Task<List<TracePlace>> GetAllAsync(string? viewerUserId = null);

    /// <summary>
    /// 추천 스팟에 공유된 장소만 조회합니다. 비회원도 이 목록은 볼 수 있습니다.
    /// </summary>
    Task<List<TracePlace>> GetSharedAsync(string? viewerUserId = null);

    /// <summary>
    /// 작성자 본인의 개인 장소 또는 추천 스팟에 공유된 장소만 조회합니다.
    /// </summary>
    Task<TracePlace?> GetByIdAsync(int id, string? viewerUserId = null);

    Task<bool> CanViewAsync(int id, string? viewerUserId = null);
    Task<bool> CanModifyAsync(int id, string? viewerUserId = null);
    Task<TracePlace> AddAsync(TracePlace place);
    Task<bool> UpdateAsync(TracePlace place);
    Task<bool> DeleteAsync(int id);
    Task<bool> MarkVisitedAsync(int id);
    Task<bool> AddVisitAsync(int id);
    Task<bool> RemoveVisitAsync(int id);
}
