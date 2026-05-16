using TraceMap.Models;

namespace TraceMap.Services;

public class ChallengeService : IChallengeService
{
    private readonly ITracePlaceService _places;

    public ChallengeService(ITracePlaceService places)
    {
        _places = places;
    }

    public async Task<List<ChallengeStatus>> GetStatusesAsync()
    {
        var places = await _places.GetAllAsync();
        var visited = places.Where(p => p.IsVisited || p.VisitCount > 0).ToList();
        var walkingVisitedCount = visited.Count(p => p.Category.Contains("산책", StringComparison.OrdinalIgnoreCase));
        var exerciseVisitedCount = visited.Count(p => p.Category.Contains("운동", StringComparison.OrdinalIgnoreCase));
        var photoPlaces = places.Count(p => p.Category.Contains("사진", StringComparison.OrdinalIgnoreCase));
        var culturePlaces = places.Count(p => p.Category.Contains("그래피티", StringComparison.OrdinalIgnoreCase) || p.Category.Contains("문화", StringComparison.OrdinalIgnoreCase));

        return
        [
            new() { Key = "add-one", Title = "새로운 스팟 1곳 기록하기", Description = "직접 발견한 장소를 하나 이상 기록합니다.", Current = Math.Min(places.Count, 1), Target = 1, IsCompleted = places.Count >= 1 },
            new() { Key = "walk-three", Title = "산책 장소 3곳 방문하기", Description = "산책 카테고리 장소를 3곳 이상 방문합니다.", Current = Math.Min(walkingVisitedCount, 3), Target = 3, IsCompleted = walkingVisitedCount >= 3 },
            new() { Key = "exercise-one", Title = "운동 스팟 방문하기", Description = "운동하기 좋은 장소를 한 곳 이상 방문합니다.", Current = Math.Min(exerciseVisitedCount, 1), Target = 1, IsCompleted = exerciseVisitedCount >= 1 },
            new() { Key = "photo-one", Title = "사진 찍기 좋은 장소 찾기", Description = "사진 카테고리 장소를 한 곳 이상 기록합니다.", Current = Math.Min(photoPlaces, 1), Target = 1, IsCompleted = photoPlaces >= 1 },
            new() { Key = "culture-one", Title = "지역 문화가 깃든 장소 다녀오기", Description = "그래피티나 지역 문화가 담긴 장소를 기록합니다.", Current = Math.Min(culturePlaces, 1), Target = 1, IsCompleted = culturePlaces >= 1 }
        ];
    }
}
