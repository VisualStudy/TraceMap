using TraceMap.Models;

namespace TraceMap.Services;

public class TracePageService : ITracePageService
{
    private static readonly List<TracePage> Pages =
    [
        new() { Key = "home", Title = "홈", Path = "/", Description = "TraceMap 시작 화면입니다.", Icon = "⌂", DisplayOrder = 1 },
        new() { Key = "map", Title = "지도 화면", Path = "/Map", Description = "저장된 장소를 지도형 화면과 마커로 확인합니다.", Icon = "📍", DisplayOrder = 2 },
        new() { Key = "places", Title = "내 장소 목록", Path = "/Places", Description = "사용자가 기록한 장소를 카드 목록으로 확인합니다.", Icon = "☰", DisplayOrder = 3 },
        new() { Key = "add-place", Title = "새 장소 추가", Path = "/Places/Create", Description = "장소 이름, 카테고리, 설명, 방문 여부, 위치를 입력합니다.", Icon = "+", DisplayOrder = 4 },
        new() { Key = "challenges", Title = "도전과제", Path = "/Challenges", Description = "장소 추가와 방문 기록에 따라 도전과제 진행도를 확인합니다.", Icon = "🏆", DisplayOrder = 5 },
        new() { Key = "recommendations", Title = "추천 스팟", Path = "/Recommendations", Description = "공유 가능한 장소와 개발자 추천 장소를 확인합니다.", Icon = "★", DisplayOrder = 6 },
        new() { Key = "profile", Title = "회원정보", Path = "/Account/Profile", Description = "로그인한 사용자 정보를 확인합니다.", RequiresAuth = true, Icon = "👤", DisplayOrder = 7 }
    ];

    public IReadOnlyList<TracePage> GetPages() => Pages.OrderBy(p => p.DisplayOrder).ToList();

    public TracePage? GetPage(string key) => Pages.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
}
