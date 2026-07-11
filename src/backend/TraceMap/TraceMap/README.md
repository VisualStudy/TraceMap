# TraceMap ASP.NET Core MVC

TraceMap의 서버 사이드 애플리케이션이자 웹 버전 프로젝트입니다.

이 프로젝트는 **ASP.NET Core MVC**를 기반으로 하며, 장소 기록과 지도 기반 탐색, 사용자 인증, 방문 기록, 도전과제, 사진 관리 등의 기능을 제공합니다. 동시에 Flutter로 개발된 TraceMap 클라이언트가 사용할 수 있는 REST API도 함께 제공하도록 구성되어 있습니다.

TraceMap은 일반적인 지도 서비스처럼 이미 등록된 상점이나 관광지를 검색하는 데 초점을 두지 않습니다. 사용자가 직접 발견한 장소에 자신만의 이름과 설명을 부여하고, 위치와 사진, 방문 기록을 함께 저장하여 **개인의 삶과 경험이 담긴 장소를 기록하는 지도 기반 플랫폼**을 목표로 합니다.

예를 들어 다음과 같은 장소를 기록할 수 있습니다.

- 노을이 잘 보이는 이름 없는 계단
- 운동하기 좋은 공터
- 산책하기 좋은 골목길
- 기억에 남는 여행지
- 개인적으로 의미가 있는 장소
- 다른 사용자에게 추천하고 싶은 장소

사용자는 장소를 지도 위에 직접 지정하고, 설명과 사진을 추가하며, 방문 여부와 방문 횟수를 관리할 수 있습니다. 공유가 허용된 장소는 다른 사용자에게 추천 스팟으로 제공할 수 있습니다.

---

## 주요 특징

### 지도 기반 장소 기록

사용자는 지도 위의 원하는 위치를 직접 선택하여 장소를 등록할 수 있습니다.

공식적으로 등록된 상점이나 관광지가 아니더라도 위도와 경도를 직접 지정하여 자신만의 장소를 기록할 수 있습니다.

지도 기능은 **Leaflet**과 **OpenStreetMap**을 기반으로 구현되어 있어 별도의 Google Maps API Key 없이 사용할 수 있습니다.

주요 지도 기능은 다음과 같습니다.

- 저장된 장소의 지도 마커 표시
- 카테고리 기반 장소 구분
- 지도에서 원하는 위치 직접 선택
- 선택한 위치의 위도와 경도 저장
- 마커를 통한 장소 정보 확인
- 지도에서 장소 상세 화면으로 이동

---

### 사용자별 개인 장소 관리

TraceMap의 장소 데이터는 로그인한 사용자를 기준으로 관리됩니다.

각 사용자는 자신이 등록한 장소만 확인하고 관리할 수 있으며, 다른 사용자의 개인 장소를 임의로 수정하거나 삭제할 수 없습니다.

장소 데이터에는 다음과 같은 정보가 포함됩니다.

- 장소 이름
- 카테고리
- 설명
- 추천 활동
- 위도
- 경도
- 방문 여부
- 방문 횟수
- 공유 여부
- 장소 사진
- 생성 사용자 정보
- 생성 및 수정 정보

이를 통해 TraceMap은 단순한 장소 목록이 아니라 사용자마다 독립적인 장소 기록 공간을 제공합니다.

---

### 장소 방문 기록

사용자는 등록한 장소를 실제로 방문했는지 기록할 수 있습니다.

방문하지 않은 장소는 방문 예정 상태로 관리할 수 있으며, 방문 완료 후에는 방문 횟수를 계속 누적할 수 있습니다.

지원하는 기능은 다음과 같습니다.

- 방문 완료 처리
- 방문 횟수 증가
- 방문 횟수 감소
- 방문 기록 취소
- 장소 목록과 상세 화면에서 방문 상태 확인

같은 장소를 여러 번 방문한 경우에도 방문 횟수를 누적하여 자주 방문하는 장소를 확인할 수 있습니다.

---

### 장소 사진 관리

장소에는 여러 장의 사진을 등록할 수 있습니다.

사진 데이터는 애플리케이션의 데이터베이스와 파일 저장소를 분리하여 관리하며, 운영 환경에서는 **Azure Blob Storage**를 사용할 수 있습니다.

지원하는 기능은 다음과 같습니다.

- 장소별 여러 장의 사진 업로드
- 장소 사진 목록 조회
- 기존 사진 교체
- 사진 삭제
- 사진 원본 조회
- Flutter 앱과 웹 환경의 사진 업로드 지원

장소 정보와 사진 파일을 분리하여 관리하기 때문에 대용량 이미지 데이터를 관계형 데이터베이스에 직접 저장하지 않고 외부 스토리지를 활용할 수 있습니다.

---

### 도전과제

TraceMap은 사용자가 새로운 장소를 기록하고 방문하도록 동기를 부여하기 위한 도전과제 기능을 제공합니다.

도전과제는 현재 사용자의 장소 데이터를 기반으로 계산됩니다.

예를 들어 다음과 같은 조건을 활용할 수 있습니다.

- 새로운 장소 등록
- 특정 카테고리 장소 방문
- 여러 장소 방문
- 일정 수 이상의 장소 기록

사용자의 장소와 방문 기록이 변경되면 도전과제 상태에도 반영될 수 있도록 구성되어 있습니다.

---

### 추천 스팟과 장소 공유

사용자는 장소를 개인 기록으로만 유지하거나 다른 사용자와 공유할 수 있도록 설정할 수 있습니다.

공유 가능한 장소는 추천 스팟 목록을 통해 다른 사용자에게 제공됩니다.

이를 통해 TraceMap은 다음 두 가지 목적을 함께 지원합니다.

1. 개인적인 장소 기록
2. 다른 사용자와의 유용한 장소 정보 공유

개인적인 기억이나 기록은 비공개로 유지하면서, 다른 사람에게 소개하고 싶은 장소만 선택적으로 공유할 수 있습니다.

---

## 프로젝트 구성

이 프로젝트는 하나의 ASP.NET Core 애플리케이션에서 두 가지 역할을 함께 수행합니다.

### MVC 웹 애플리케이션

브라우저에서 직접 사용할 수 있는 TraceMap 웹 인터페이스를 제공합니다.

주요 MVC 화면은 다음과 같습니다.

- 홈 화면
- 지도 화면
- 지도에서 위치 선택
- 내 장소 목록
- 장소 상세 정보
- 새 장소 추가
- 장소 수정
- 도전과제
- 추천 스팟
- 로그인
- 회원가입
- 회원정보

---

### REST API 서버

Flutter로 개발된 TraceMap 앱과 외부 클라이언트가 사용할 수 있는 REST API를 제공합니다.

MVC 웹 버전과 Flutter 앱은 동일한 데이터베이스와 서비스 계층을 사용할 수 있으므로, 웹과 모바일 환경에서 동일한 장소 데이터를 관리할 수 있습니다.

---

## 기술 스택

### Backend

- ASP.NET Core
- ASP.NET Core MVC
- ASP.NET Core Web API
- C#
- Entity Framework Core
- ASP.NET Core Identity

### Database

- Entity Framework Core InMemory Database
- Microsoft Azure SQL Database

### Cloud

- Azure Web App
- Azure SQL Database
- Azure Blob Storage

### Map

- Leaflet
- OpenStreetMap

### Client Integration

- MVC Web Client
- Flutter Mobile/Web Client
- REST API
- Bearer Token Authentication

---

## 애플리케이션 구조

TraceMap 백엔드는 역할별로 코드를 분리하여 관리합니다.

일반적인 구조는 다음과 같습니다.

```text
TraceMap
├── Controllers
│   ├── MVC Controllers
│   └── API Controllers
├── Data
│   ├── ApplicationDbContext
│   └── EF Core Migrations
├── Models
│   ├── ApplicationUser
│   ├── TracePlace
│   └── PlacePhoto
├── Services
│   ├── Place Service
│   ├── Challenge Service
│   ├── Photo Storage Service
│   └── Seed Data Service
├── Views
│   ├── Home
│   ├── Places
│   ├── Map
│   ├── Challenges
│   └── Account
├── wwwroot
├── Program.cs
└── appsettings.json
```

컨트롤러가 모든 데이터 처리 로직을 직접 수행하지 않고, 주요 비즈니스 로직을 서비스 계층으로 분리하여 관리하도록 구성되어 있습니다.

---

## 인증 구조

TraceMap은 웹 브라우저와 Flutter 앱의 서로 다른 사용 방식을 고려하여 인증 방식을 구성합니다.

### MVC 웹

MVC 웹 화면에서는 쿠키 기반 인증을 사용합니다.

로그인하지 않은 사용자가 인증이 필요한 MVC 페이지에 접근하면 로그인 페이지로 이동합니다.

예:

```text
/Places
/Map
/Challenges
/Account/Profile
```

인증되지 않은 사용자가 위와 같은 보호된 페이지에 접근하면 다음 로그인 화면으로 연결됩니다.

```text
/Account/Login
```

로그인 후에는 원래 접근하려던 페이지로 돌아갈 수 있습니다.

---

### Flutter 및 API

Flutter 앱에서는 Bearer Token 기반 Identity API를 사용할 수 있습니다.

예:

```http
POST /api/identity/login?useCookies=false
Content-Type: application/json

{
  "email": "administrator@tracemap.com",
  "password": "Pa$$w0rd"
}
```

로그인에 성공하면 발급받은 인증 정보를 이용하여 보호된 API를 호출할 수 있습니다.

이 구조를 통해 하나의 ASP.NET Core 서버에서 다음 인증 방식을 함께 지원합니다.

```text
MVC Web
→ Cookie Authentication

Flutter / REST API
→ Bearer Token Authentication
```

---

## 데이터베이스 구성

TraceMap은 실행 환경에 따라 서로 다른 데이터베이스를 사용합니다.

### Development

로컬 개발 환경에서는 EF Core InMemory Database를 사용합니다.

```csharp
UseInMemoryDatabase("TraceMapDb")
```

따라서 별도의 SQL Server를 설치하거나 데이터베이스 연결 문자열을 설정하지 않아도 프로젝트를 바로 실행할 수 있습니다.

단, InMemory Database는 애플리케이션 프로세스가 종료되면 데이터가 유지되지 않습니다.

로컬 환경은 빠른 개발과 테스트를 위한 용도로 사용합니다.

---

### Production

Azure Web App과 같은 운영 환경에서는 **Azure SQL Database**를 사용합니다.

연결 문자열은 다음 구성 값을 통해 가져옵니다.

```text
ConnectionStrings__DefaultConnection
```

운영 환경에서는 애플리케이션 시작 시 EF Core 마이그레이션을 적용합니다.

```csharp
await db.Database.MigrateAsync();
```

따라서 새로운 데이터베이스가 연결된 경우 필요한 테이블을 생성하고, 기존 데이터베이스가 있는 경우 최신 마이그레이션 상태로 업데이트할 수 있습니다.

---

## Azure SQL Database 설정

Azure Web App의 Configuration 또는 Environment Variables에 다음 값을 설정합니다.

```text
ConnectionStrings__DefaultConnection=Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

실제 연결 문자열과 비밀번호 같은 민감한 정보는 소스 코드나 GitHub 저장소에 직접 포함하지 않는 것을 권장합니다.

Azure Web App의 환경 변수 또는 별도의 Secret 관리 기능을 사용하여 관리해야 합니다.

---

## Azure Blob Storage

장소 사진은 Azure Blob Storage에 저장할 수 있습니다.

다음 설정 중 하나를 사용할 수 있습니다.

```text
PlacePhotoStorage__ConnectionString=<Azure Blob Storage connection string>
```

또는:

```text
ConnectionStrings__AzureBlobStorage=<Azure Blob Storage connection string>
```

Azure Blob Storage를 사용하면 사진 파일 자체는 Blob Storage에 저장하고, 애플리케이션에서는 장소와 사진의 관계 및 필요한 메타데이터를 관리할 수 있습니다.

스토리지 연결 문자열 역시 GitHub와 같은 공개 저장소에 직접 포함하지 않고 Azure Web App 환경 변수로 관리해야 합니다.

---

## 기본 데이터

애플리케이션 시작 시 Seed Data Service를 통해 필요한 초기 데이터를 생성합니다.

기본 관리자 계정은 다음과 같습니다.

```text
Email: administrator@tracemap.com
Password: Pa$$w0rd
```

로컬 개발 및 테스트를 위한 기본 계정입니다.

운영 환경에서 기본 계정을 사용할 경우 로그인 후 반드시 비밀번호를 변경하는 것을 권장합니다.

기본 계정에는 TraceMap의 기능을 바로 확인할 수 있도록 샘플 장소 데이터가 생성될 수 있습니다.

---

## 로컬 실행

`TraceMap.csproj`가 있는 프로젝트 폴더에서 다음 명령을 실행합니다.

```bash
dotnet restore
dotnet run
```

실행 후 브라우저에서 다음 주소 또는 콘솔에 출력된 URL로 접속합니다.

```text
http://localhost:5078
```

HTTPS 프로필을 사용하는 경우 다음과 같은 주소가 사용될 수 있습니다.

```text
https://localhost:7258
```

실제 포트는 `launchSettings.json` 또는 실행 환경에 따라 달라질 수 있으므로 `dotnet run` 실행 로그를 확인하는 것이 가장 정확합니다.

---

## EF Core 마이그레이션

새로운 데이터베이스 구조 변경 사항을 마이그레이션으로 생성하려면 `TraceMap.csproj`가 있는 폴더에서 다음 명령을 실행합니다.

```bash
dotnet ef migrations add AddSomething
```

예:

```bash
dotnet ef migrations add AddPlacePhotos
```

데이터베이스에 직접 마이그레이션을 적용하려면 다음 명령을 사용할 수 있습니다.

```bash
dotnet ef database update
```

현재 Production 환경에서는 애플리케이션 시작 시 다음 코드가 실행됩니다.

```csharp
await db.Database.MigrateAsync();
```

따라서 Azure Web App에 새로운 버전을 배포하면 연결된 Azure SQL Database에 아직 적용되지 않은 마이그레이션이 자동으로 반영됩니다.

---

## 주요 MVC 기능

### 장소 관리

* 내 장소 목록 조회
* 장소 상세 조회
* 새 장소 등록
* 장소 수정
* 장소 삭제
* 사용자별 장소 데이터 분리
* 장소 소유자 기반 수정 및 삭제 권한 관리

### 위치 및 지도

* Leaflet 지도 표시
* OpenStreetMap 타일 사용
* 장소별 마커 표시
* 지도에서 위치 선택
* 위도와 경도 저장
* 선택한 장소 상세 화면 이동

### 방문 기록

* 방문 완료
* 방문 횟수 증가
* 방문 횟수 감소
* 방문 상태 변경

### 장소 공유

* 개인 장소 관리
* 공유 가능한 장소 지정
* 추천 스팟 목록 제공

### 사진

* 여러 장의 장소 사진 업로드
* 사진 목록 조회
* 사진 교체
* 사진 삭제
* 사진 콘텐츠 조회

### 사용자

* 회원가입
* 로그인
* 로그아웃
* 회원정보
* 인증이 필요한 MVC 페이지 보호

### 도전과제

* 현재 장소 데이터를 기반으로 진행 상태 계산
* 장소 등록 및 방문 기록과 연계 가능한 도전과제 제공

---

## REST API

### Pages

```http
GET /api/pages
GET /api/pages/{key}
```

---

### Places

```http
GET    /api/places
GET    /api/places/shared
GET    /api/places/{id}
POST   /api/places
PUT    /api/places/{id}
DELETE /api/places/{id}
```

---

### Visit Records

```http
POST /api/places/{id}/mark-visited
POST /api/places/{id}/visit-plus
POST /api/places/{id}/visit-minus
```

---

### Place Photos

```http
GET    /api/places/{placeId}/photos
POST   /api/places/{placeId}/photos
PUT    /api/places/{placeId}/photos/{photoId}
DELETE /api/places/{placeId}/photos/{photoId}
GET    /api/places/{placeId}/photos/{photoId}/content
```

---

### Challenges

```http
GET /api/challenges
```

---

### Authentication

```http
GET /api/auth/me
GET /api/secure
```

ASP.NET Core Identity API는 다음 경로를 사용합니다.

```text
/api/identity/*
```

대표적으로 로그인 API를 다음과 같이 사용할 수 있습니다.

```http
POST /api/identity/login?useCookies=false
Content-Type: application/json

{
  "email": "administrator@tracemap.com",
  "password": "Pa$$w0rd"
}
```

---

## 지도 서비스

TraceMap 웹 버전은 **Leaflet과 OpenStreetMap**을 사용합니다.

이 방식의 주요 장점은 다음과 같습니다.

* 별도의 Google Maps API Key가 필요하지 않음
* 개발 및 테스트를 빠르게 시작할 수 있음
* 지도 마커 표시 가능
* 사용자가 지도 위의 위치를 직접 선택할 수 있음
* 위도와 경도를 기반으로 사용자 정의 장소를 저장할 수 있음

인터넷 연결이 가능한 환경에서는 별도의 지도 API Key 설정 없이 지도 기능을 확인할 수 있습니다.

TraceMap은 일반 지도 서비스에 공식적으로 등록되지 않은 장소도 직접 기록할 수 있어야 하므로, 지도에서 좌표를 직접 지정하는 기능은 프로젝트의 핵심 기능 중 하나입니다.

---

## MVC와 Flutter의 연동

TraceMap 백엔드는 MVC 웹 애플리케이션만을 위한 서버가 아닙니다.

동일한 ASP.NET Core 애플리케이션에서 REST API를 함께 제공하여 Flutter로 개발된 TraceMap 앱에서도 데이터를 사용할 수 있도록 구성되어 있습니다.

전체적인 구조는 다음과 같습니다.

```text
┌──────────────────────┐
│   MVC Web Browser    │
│  Cookie Authentication
└──────────┬───────────┘
           │
           │
           ▼
┌──────────────────────┐
│ ASP.NET Core TraceMap│
│                      │
│ MVC Controllers      │
│ API Controllers      │
│ Services             │
│ ASP.NET Core Identity│
│ Entity Framework Core│
└──────────┬───────────┘
           │
     ┌─────┴─────┐
     │           │
     ▼           ▼
┌──────────┐ ┌──────────────┐
│Azure SQL │ │ Azure Blob   │
│ Database │ │   Storage    │
└──────────┘ └──────────────┘
           ▲
           │
           │
┌──────────┴───────────┐
│    Flutter Client    │
│ Bearer Token / API   │
└──────────────────────┘
```

이를 통해 웹과 Flutter 앱이 서로 다른 UI를 사용하면서도 동일한 서버와 데이터 구조를 공유할 수 있습니다.

---

## 배포 구조

운영 환경에서는 다음과 같은 Azure 기반 구성을 사용할 수 있습니다.

```text
User
  │
  ├── Web Browser
  │
  └── Flutter Application
          │
          ▼
   Azure Web App
   ASP.NET Core
          │
          ├── Azure SQL Database
          │     └── 사용자, 장소, 방문 기록, 사진 메타데이터
          │
          └── Azure Blob Storage
                └── 장소 이미지 파일
```

ASP.NET Core 애플리케이션은 Azure Web App에서 실행되고, 관계형 데이터는 Azure SQL Database에 저장됩니다.

장소 사진과 같은 파일 데이터는 Azure Blob Storage를 활용하여 관리할 수 있습니다.

---

## 프로젝트 목표

TraceMap은 단순한 CRUD 예제나 지도 테스트 프로젝트가 아닙니다.

이 프로젝트가 지향하는 핵심 가치는 사용자가 살아가면서 발견한 장소를 자신의 경험과 함께 기록할 수 있도록 하는 것입니다.

일반적인 지도 서비스가 다음과 같은 질문에 답한다면:

> 어디에 식당이 있는가?
> 어디에 카페가 있는가?
> 목적지까지 어떻게 이동하는가?

TraceMap은 다음과 같은 질문에 답하는 것을 목표로 합니다.

> 내가 기억하고 싶은 장소는 어디인가?
> 이 장소가 나에게 어떤 의미가 있는가?
> 나는 어떤 장소를 자주 방문했는가?
> 다른 사람에게 추천하고 싶은 장소는 어디인가?

이를 위해 TraceMap은 지도, 장소 기록, 사용자 계정, 방문 기록, 사진, 공유 기능을 하나의 애플리케이션 안에서 연결합니다.

궁극적으로는 사용자의 장소와 이동 경험을 축적하여 **개인의 삶과 경험이 지도 위에 기록되는 서비스**를 목표로 합니다.
