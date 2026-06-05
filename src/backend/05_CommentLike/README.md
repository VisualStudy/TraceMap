# TraceMap 05_CommentLike

04_Refactoring 소스를 기반으로 장소 상세 화면의 댓글과 좋아요 기능을 추가한 단계입니다.

## 추가된 내용

- `PlaceComment`, `PlaceLike` 모델 추가
- `ApplicationDbContext`에 `PlaceComments`, `PlaceLikes` DbSet 추가
- `api/places/{placeId}/social` 조회 API 추가
- `api/places/{placeId}/like` 좋아요 토글 API 추가
- `api/places/{placeId}/comments` 댓글 등록 API 추가
- 좋아요와 댓글 등록은 `[Authorize]` 적용
- 장소 생성/수정 시 로그인 사용자는 `UserId`, `UserName` 저장, 비로그인 사용자는 익명 데이터로 저장
