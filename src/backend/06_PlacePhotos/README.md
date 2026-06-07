# TraceMap Backend 06_PlacePhotos

`05_CommentLike`를 기준으로 장소별 다중 사진 업로드 기능을 추가한 버전입니다.

## 추가 기능

- `PlacePhoto` EF Core In-Memory 모델 추가
- `ApplicationDbContext.PlacePhotos` 추가
- 장소 목록/상세 응답에 `PhotoCount` 포함
- `api/places/{placeId}/photos` API 추가
  - `GET` 사진 목록
  - `POST` 다중 사진 업로드, 로그인 필요
  - `PUT {photoId}` 사진 교체, 로그인 필요
  - `DELETE {photoId}` 사진 삭제, 로그인 필요
  - `GET {photoId}/content` 사진 뷰어 다운로드/표시
- 기본 저장소: `wwwroot/place-photos/{PlaceId}/{Guid}.jpg`
- Blob 저장소: `place-photos` 컨테이너에 `{PlaceId}/{Guid}.jpg` 형태로 함께 업로드
- Blob 연결 정보가 없거나 잘못되어도 로컬 저장은 유지되고, 다른 기능은 정상 동작하도록 예외 처리

## 설정 예시

```json
{
  "PlacePhotoStorage": {
    "ContainerName": "place-photos",
    "LocalRoot": "place-photos",
    "ConnectionString": "",
    "EnableBlobUpload": true
  },
  "ConnectionStrings": {
    "AzureBlobStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
  }
}
```

또는 기존 코드 패턴처럼 아래 AppKeys 방식도 지원합니다.

```json
{
  "AppKeys": {
    "AzureStorageAccount": "...",
    "AzureStorageAccessKey": "...",
    "AzureStorageEndpointSuffix": "core.windows.net"
  }
}
```
