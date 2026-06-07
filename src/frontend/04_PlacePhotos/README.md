# TraceMap Frontend 04_PlacePhotos

`03_CommentLike`를 기준으로 장소 상세 화면에 사진 업로드 버튼과 사진 갤러리를 추가한 버전입니다.

## 추가 기능

- `image_picker` 패키지 추가
- 장소 상세 화면에서 로그인 사용자만 사진 업로드 가능
- 사진 목록은 `api/places/{placeId}/photos`에서 조회
- 사진 이미지는 Blob URL을 직접 읽지 않고 백엔드 뷰어 API인 `api/places/{placeId}/photos/{photoId}/content`를 통해 표시
- 사진 삭제 기능 추가
- 장소 목록 카드에 사진 개수 표시

## 사용 전 확인

`lib/main.dart`의 `apiBaseUrl`이 현재 배포된 백엔드 주소와 맞는지 확인하세요.
