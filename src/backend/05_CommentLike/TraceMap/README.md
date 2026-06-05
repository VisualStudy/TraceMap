# TraceMap ASP.NET Core MVC

첨부 기획서의 TraceMap 흐름을 ASP.NET Core MVC와 Web API로 먼저 구현한 테스트 프로젝트입니다.

## 실행

```bash
dotnet restore
dotnet run
```

브라우저에서 `http://localhost:5078` 또는 실행 로그의 URL로 접속합니다.

## 기본 계정

- Email: `administrator@tracemap.com`
- Password: `Pa$$w0rd`

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
- `GET /api/challenges`
- `GET /api/secure`
- `GET /api/auth/me`
- `/api/identity/*` Identity API

## 참고

DB는 `UseInMemoryDatabase("TraceMapDb")`를 사용합니다. 앱을 재시작하면 시드 데이터가 다시 만들어집니다.


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
