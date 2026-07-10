# TraceMap ASP.NET Core MVC

TraceMap 백엔드 테스트 프로젝트입니다. 배포 환경에서는 Azure SQL Database를 사용하도록 업그레이드한 버전입니다.

## 실행

```bash
dotnet restore
dotnet run
```

브라우저에서 `http://localhost:5078` 또는 실행 로그의 URL로 접속합니다.

## 기본 계정

- Email: `administrator@tracemap.com`
- Password: `Pa$$w0rd`

실제 운영 환경에서는 위 계정으로 로그인 후 `회원정보` 화면에서 비밀번호를 변경하시기 바랍니다.

## DB 동작 방식

- 로컬 `Development` 환경: `UseInMemoryDatabase("TraceMapDb")` 사용
- Azure Web App 등 `Production` 환경: `ConnectionStrings__DefaultConnection` 값으로 Azure SQL Database 연결
- Production 시작 시 `Database.MigrateAsync()`를 실행하여 EF Core 마이그레이션을 적용
- 이후 기존 샘플 사용자와 샘플 장소 데이터를 시드 처리

Azure Web App의 **Configuration > Application settings**에 다음 값을 추가합니다.

```text
ConnectionStrings__DefaultConnection=Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

사진 업로드용 Azure Blob Storage 연결 문자열은 기존처럼 다음 중 하나로 설정할 수 있습니다.

```text
PlacePhotoStorage__ConnectionString=<Azure Blob Storage connection string>
```

또는

```text
ConnectionStrings__AzureBlobStorage=<Azure Blob Storage connection string>
```

## EF Core 마이그레이션 명령

새로운 마이그레이션을 추가할 때는 `TraceMap.csproj`가 있는 폴더에서 실행합니다.

```bash
dotnet ef migrations add AddSomething
```

Azure SQL Database에 직접 반영하려면 연결 문자열을 설정한 뒤 실행합니다.

```bash
dotnet ef database update
```

현재 배포 흐름에서는 앱 시작 시 Production 환경에서 자동으로 `Database.MigrateAsync()`를 호출하므로, Azure Web App에 게시하면 앱이 스키마를 생성하거나 최신 마이그레이션까지 업데이트합니다.

## 구현된 MVC 화면

- 홈 화면
- 지도 화면: Leaflet + OpenStreetMap 무료 지도와 카테고리별 마커 구현
- 지도에서 위치 선택 화면: 지도를 클릭하여 위도/경도를 선택하고 장소 추가 화면으로 전달
- 내 장소 목록 화면
- 장소 상세 화면
- 장소 추가 화면
- 장소 수정 화면
- 방문 완료, 방문 횟수 +1, 방문 횟수 -1
- 삭제 확인 후 삭제
- 도전과제 화면: 장소 데이터 기반 자동 계산
- 추천 스팟 화면: 공유 가능한 장소 목록
- 회원정보 화면: 인증 필요
- 장소 사진 여러 장 업로드, 목록 조회, 교체, 삭제, 보기 API

## 구현된 API

- `GET /api/pages`
- `GET /api/pages/{key}`
- `GET /api/places`
- `GET /api/places/shared`
- `GET /api/places/{id}`
- `POST /api/places`
- `PUT /api/places/{id}`
- `DELETE /api/places/{id}`
- `POST /api/places/{id}/mark-visited`
- `POST /api/places/{id}/visit-plus`
- `POST /api/places/{id}/visit-minus`
- `GET /api/places/{placeId}/photos`
- `POST /api/places/{placeId}/photos`
- `PUT /api/places/{placeId}/photos/{photoId}`
- `DELETE /api/places/{placeId}/photos/{photoId}`
- `GET /api/places/{placeId}/photos/{photoId}/content`
- `GET /api/challenges`
- `GET /api/secure`
- `GET /api/auth/me`
- `/api/identity/*` Identity API

## 지도 구현

이 버전은 Google Maps API Key 없이 테스트할 수 있도록 Leaflet과 OpenStreetMap 타일을 사용합니다. 인터넷 연결이 가능한 환경에서는 별도 키 설정 없이 지도, 마커 선택, 위치 선택 기능을 바로 확인할 수 있습니다.

## 인증 MVC 화면

우측 상단 인증 메뉴를 추가했습니다.

- 로그아웃 상태: `로그인`, `회원가입`
- 로그인 상태: `회원정보`, `로그아웃`
- 회원정보 화면: `/Account/Profile`
- 로그인 화면: `/Account/Login`
- 회원가입 화면: `/Account/Register`

MVC 화면에서는 쿠키 인증을 사용하고, Flutter 연동 API에서는 기존처럼 Bearer Token 기반 Identity API를 사용할 수 있습니다.

```http
POST /api/identity/login?useCookies=false
Content-Type: application/json

{
  "email": "administrator@tracemap.com",
  "password": "Pa$$w0rd"
}
```
