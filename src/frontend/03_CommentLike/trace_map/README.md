# TraceMap Flutter Frontend

TraceMap의 Android Flutter 프론트엔드입니다.

- API Base URL: `https://tracemap.azurewebsites.net`
- 인증: ASP.NET Core Identity API(`/api/identity/register`, `/api/identity/login`) 사용
- 사용자 확인: `/api/auth/me`
- 장소 CRUD: `/api/places`
- 방문 기록: `/api/places/{id}/mark-visited`, `/visit-plus`, `/visit-minus`
- 도전과제: `/api/challenges`
- 지도: `flutter_map` + OpenStreetMap 타일 사용

## 실행 방법

1. 압축을 풉니다.
2. Android Studio에서 이 Flutter 프로젝트 폴더를 엽니다.
3. 터미널에서 `flutter pub get`을 실행합니다.
4. Pixel 6 같은 Android 에뮬레이터를 실행합니다.
5. `flutter run` 또는 Android Studio의 Run 버튼으로 실행합니다.

Android 인터넷 권한은 `android/app/src/main/AndroidManifest.xml`에 포함되어 있습니다.
