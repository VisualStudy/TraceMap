import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:image_picker/image_picker.dart';
import 'package:latlong2/latlong.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() {
  runApp(const MyApp());
}

const String apiBaseUrl = 'https://tracemap.azurewebsites.net';

class MyApp extends StatefulWidget {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  final ApiClient api = ApiClient(apiBaseUrl);
  List<TracePlace> places = [];
  List<TracePlace> sharedPlaces = [];
  List<ChallengeStatus> challenges = [];
  AuthUser? user;
  bool isLoading = true;
  int selectedIndex = 0;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    await api.loadSavedToken();
    await refreshUser(silent: true);
    await refreshAll(showLoading: true);
  }

  Future<void> refreshAll({bool showLoading = false}) async {
    if (showLoading) setState(() => isLoading = true);
    try {
      final results = await Future.wait([
        api.getPlaces(),
        api.getSharedPlaces(),
        api.getChallenges(),
      ]);
      setState(() {
        places = results[0] as List<TracePlace>;
        sharedPlaces = results[1] as List<TracePlace>;
        challenges = results[2] as List<ChallengeStatus>;
        isLoading = false;
      });
    } catch (e) {
      setState(() => isLoading = false);
      if (mounted) showMessage(context, '데이터를 불러오지 못했습니다: $e');
    }
  }

  Future<void> refreshUser({bool silent = false}) async {
    if (!api.isSignedIn) {
      setState(() => user = null);
      return;
    }
    try {
      final me = await api.getMe();
      setState(() => user = me);
    } catch (_) {
      await api.clearToken();
      setState(() => user = null);
      if (!silent && mounted) showMessage(context, '로그인 정보가 만료되었습니다. 다시 로그인해 주세요.');
    }
  }

  Future<void> onLoggedIn(AuthUser? me) async {
    setState(() {
      user = me;
      selectedIndex = 0;
    });
    await refreshAll();
  }

  Future<void> logout() async {
    await api.clearToken();
    setState(() {
      user = null;
      places = [];
      challenges = [];
      selectedIndex = 0;
    });
    await refreshAll();
  }

  @override
  Widget build(BuildContext context) {
    final color = ColorScheme.fromSeed(seedColor: const Color(0xff2563eb));
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'TraceMap',
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: color,
        scaffoldBackgroundColor: const Color(0xfff3f7ff),
        appBarTheme: AppBarTheme(
          backgroundColor: color.primary,
          foregroundColor: color.onPrimary,
          centerTitle: true,
        ),
      ),
      home: isLoading
          ? const LoadingScreen()
          : TraceShell(
              api: api,
              places: places,
              sharedPlaces: sharedPlaces,
              challenges: challenges,
              user: user,
              selectedIndex: selectedIndex,
              onTabChanged: (value) => setState(() => selectedIndex = value),
              onRefresh: refreshAll,
              onLogin: onLoggedIn,
              onLogout: logout,
            ),
    );
  }
}

class TraceShell extends StatelessWidget {
  const TraceShell({
    super.key,
    required this.api,
    required this.places,
    required this.sharedPlaces,
    required this.challenges,
    required this.user,
    required this.selectedIndex,
    required this.onTabChanged,
    required this.onRefresh,
    required this.onLogin,
    required this.onLogout,
  });

  final ApiClient api;
  final List<TracePlace> places;
  final List<TracePlace> sharedPlaces;
  final List<ChallengeStatus> challenges;
  final AuthUser? user;
  final int selectedIndex;
  final ValueChanged<int> onTabChanged;
  final Future<void> Function({bool showLoading}) onRefresh;
  final Future<void> Function(AuthUser? user) onLogin;
  final Future<void> Function() onLogout;

  @override
  Widget build(BuildContext context) {
    final screens = [
      HomeScreen(places: places, challenges: challenges, user: user, onTabChanged: onTabChanged, onRefresh: onRefresh),
      MapScreen(places: places, isSignedIn: user != null, onRefresh: onRefresh),
      PlacesScreen(api: api, places: places, onRefresh: onRefresh),
      ChallengesScreen(challenges: challenges, isSignedIn: user != null, onRefresh: onRefresh),
      RecommendationsScreen(places: sharedPlaces, onRefresh: onRefresh),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('TraceMap'),
        actions: [
          IconButton(
            tooltip: user == null ? '로그인' : '회원정보',
            icon: Icon(user == null ? Icons.login : Icons.account_circle),
            onPressed: () async {
              if (user == null) {
                await Navigator.push(context, MaterialPageRoute(builder: (_) => AuthScreen(api: api, onLogin: onLogin)));
              } else {
                await Navigator.push(context, MaterialPageRoute(builder: (_) => ProfileScreen(user: user!, onLogout: onLogout)));
              }
            },
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () => onRefresh(showLoading: false),
        child: screens[selectedIndex],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: selectedIndex,
        onDestinationSelected: onTabChanged,
        destinations: const [
          NavigationDestination(icon: Icon(Icons.home_outlined), selectedIcon: Icon(Icons.home), label: '홈'),
          NavigationDestination(icon: Icon(Icons.map_outlined), selectedIcon: Icon(Icons.map), label: '지도'),
          NavigationDestination(icon: Icon(Icons.list_alt_outlined), selectedIcon: Icon(Icons.list_alt), label: '목록'),
          NavigationDestination(icon: Icon(Icons.emoji_events_outlined), selectedIcon: Icon(Icons.emoji_events), label: '도전'),
          NavigationDestination(icon: Icon(Icons.star_border), selectedIcon: Icon(Icons.star), label: '추천'),
        ],
      ),
    );
  }
}

class LoadingScreen extends StatelessWidget {
  const LoadingScreen({super.key});
  @override
  Widget build(BuildContext context) => const Scaffold(body: Center(child: CircularProgressIndicator()));
}

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key, required this.places, required this.challenges, required this.user, required this.onTabChanged, required this.onRefresh});
  final List<TracePlace> places;
  final List<ChallengeStatus> challenges;
  final AuthUser? user;
  final ValueChanged<int> onTabChanged;
  final Future<void> Function({bool showLoading}) onRefresh;

  @override
  Widget build(BuildContext context) {
    final isSignedIn = user != null;
    final completed = isSignedIn ? challenges.where((c) => c.isCompleted).length : 0;
    final visited = isSignedIn ? places.where((p) => p.isVisited).length : 0;
    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        const SizedBox(height: 16),
        const Icon(Icons.location_on, size: 62, color: Color(0xff2563eb)),
        const Text('TraceMap', textAlign: TextAlign.center, style: TextStyle(fontSize: 40, fontWeight: FontWeight.w900)),
        const Text('나의 발자취를 지도에 남기다', textAlign: TextAlign.center, style: TextStyle(color: Colors.black54)),
        const SizedBox(height: 20),
        Row(children: [
          Expanded(child: StatCard(title: '기록 장소', value: isSignedIn ? '${places.length}' : '-')),
          Expanded(child: StatCard(title: '방문 완료', value: isSignedIn ? '$visited' : '-')),
          Expanded(child: StatCard(title: '도전과제', value: isSignedIn ? '$completed/${challenges.length}' : '-')),
        ]),
        const SizedBox(height: 20),
        MenuTile(icon: Icons.map, title: '지도에서 스팟 보기', onTap: () => onTabChanged(1)),
        MenuTile(icon: Icons.list_alt, title: '내 장소 목록 보기', onTap: () => onTabChanged(2)),
        MenuTile(icon: Icons.emoji_events, title: '도전과제 확인하기', onTap: () => onTabChanged(3)),
        MenuTile(icon: Icons.star, title: '추천 스팟 보기', onTap: () => onTabChanged(4)),
        const SizedBox(height: 16),
        InfoCard(
          title: user == null ? '인증 기능' : '${user!.displayName}님, 환영합니다.',
          body: user == null
              ? '기록 장소 카운트, 내 장소 목록, 지도, 도전과제는 로그인 후 본인 계정 기준으로 제공됩니다. 비회원은 추천 스팟에 공유된 장소만 볼 수 있습니다.'
              : '현재 라이브 Azure Web API에 로그인된 상태입니다. 내 장소 목록과 지도에는 내가 등록한 장소만 표시되고, 공유한 장소만 추천 스팟에 공개됩니다.',
        ),
      ],
    );
  }
}

class StatCard extends StatelessWidget {
  const StatCard({super.key, required this.title, required this.value});
  final String title;
  final String value;
  @override
  Widget build(BuildContext context) => Card(
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(children: [
            Text(value, style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: Theme.of(context).colorScheme.primary)),
            Text(title, style: const TextStyle(fontSize: 12)),
          ]),
        ),
      );
}

class MenuTile extends StatelessWidget {
  const MenuTile({super.key, required this.icon, required this.title, required this.onTap});
  final IconData icon;
  final String title;
  final VoidCallback onTap;
  @override
  Widget build(BuildContext context) => Card(
        child: ListTile(
          leading: Icon(icon, color: Theme.of(context).colorScheme.primary),
          title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold)),
          trailing: const Icon(Icons.chevron_right),
          onTap: onTap,
        ),
      );
}

class InfoCard extends StatelessWidget {
  const InfoCard({super.key, required this.title, required this.body});
  final String title;
  final String body;
  @override
  Widget build(BuildContext context) => Card(
        color: const Color(0xffeff6ff),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(title, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(body),
          ]),
        ),
      );
}

class MapScreen extends StatefulWidget {
  const MapScreen({super.key, required this.places, required this.isSignedIn, required this.onRefresh});
  final List<TracePlace> places;
  final bool isSignedIn;
  final Future<void> Function({bool showLoading}) onRefresh;
  @override
  State<MapScreen> createState() => _MapScreenState();
}

class _MapScreenState extends State<MapScreen> {
  TracePlace? selected;

  @override
  Widget build(BuildContext context) {
    if (!widget.isSignedIn) {
      return ListView(
        padding: const EdgeInsets.all(16),
        children: const [
          ScreenHeader(title: '지도 화면', subtitle: '내가 기록한 장소의 마커는 로그인 후 확인할 수 있습니다.'),
          InfoCard(title: '로그인 필요', body: '비회원은 개인 장소 지도를 볼 수 없습니다. 공개된 장소는 추천 스팟에서 확인해 주세요.'),
        ],
      );
    }

    final places = widget.places.where((p) => p.latitude != 0 && p.longitude != 0).toList();
    final center = places.isNotEmpty ? LatLng(places.first.latitude, places.first.longitude) : const LatLng(34.9501, 127.4872);
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        const ScreenHeader(title: '지도 화면', subtitle: 'OpenStreetMap 지도에서 내가 등록한 장소의 마커만 확인합니다.'),
        ClipRRect(
          borderRadius: BorderRadius.circular(18),
          child: SizedBox(
            height: 430,
            child: FlutterMap(
              options: MapOptions(initialCenter: center, initialZoom: 13),
              children: [
                TileLayer(
                  urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                  userAgentPackageName: 'com.example.trace_map',
                ),
                MarkerLayer(
                  markers: places.map((p) => Marker(
                    point: LatLng(p.latitude, p.longitude),
                    width: 44,
                    height: 44,
                    child: GestureDetector(
                      onTap: () => setState(() => selected = p),
                      child: CircleAvatar(
                        backgroundColor: p.id == selected?.id ? Colors.orange : Theme.of(context).colorScheme.primary,
                        child: Icon(categoryIcon(p.category), color: Colors.white, size: 22),
                      ),
                    ),
                  )).toList(),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 12),
        if (selected == null)
          const InfoCard(title: '마커 선택', body: '지도 위 마커를 누르면 장소 이름, 카테고리, 설명이 표시됩니다.')
        else
          PlaceSummaryCard(place: selected!, onTap: () => openDetails(context, selected!, widget.onRefresh)),
      ],
    );
  }
}

class PlacesScreen extends StatelessWidget {
  const PlacesScreen({super.key, required this.api, required this.places, required this.onRefresh});
  final ApiClient api;
  final List<TracePlace> places;
  final Future<void> Function({bool showLoading}) onRefresh;

  @override
  Widget build(BuildContext context) => ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Row(children: [
            const Expanded(child: ScreenHeader(title: '내 장소 목록', subtitle: '로그인한 계정이 직접 추가한 장소만 표시합니다.')),
            FilledButton.icon(
              onPressed: () async {
                await Navigator.push(context, MaterialPageRoute(builder: (_) => PlaceFormScreen(api: api, onSaved: onRefresh)));
              },
              icon: const Icon(Icons.add),
              label: const Text('추가'),
            ),
          ]),
          if (!api.isSignedIn)
            const InfoCard(title: '로그인 필요', body: '내 장소 목록은 로그인한 사용자만 사용할 수 있습니다. 공개된 장소는 추천 스팟에서 확인해 주세요.')
          else if (places.isEmpty)
            const InfoCard(title: '장소 없음', body: '오른쪽 위 추가 버튼으로 새 장소를 기록해 보세요.')
          else
            ...places.map((p) => PlaceSummaryCard(place: p, onTap: () => openDetails(context, p, onRefresh))),
        ],
      );
}

class PlaceSummaryCard extends StatelessWidget {
  const PlaceSummaryCard({super.key, required this.place, required this.onTap});
  final TracePlace place;
  final VoidCallback onTap;
  @override
  Widget build(BuildContext context) => Card(
        child: ListTile(
          leading: CircleAvatar(child: Icon(categoryIcon(place.category))),
          title: Text(place.name, style: const TextStyle(fontWeight: FontWeight.bold)),
          subtitle: Text('${place.category}\n방문 여부: ${place.isVisited ? '완료' : '예정'} · 방문 ${place.visitCount}회\n좋아요 ${place.likeCount}개 · 댓글 ${place.commentCount}개 · 사진 ${place.photoCount}장'),
          isThreeLine: true,
          trailing: const Icon(Icons.chevron_right),
          onTap: onTap,
        ),
      );
}

Future<void> openDetails(BuildContext context, TracePlace place, Future<void> Function({bool showLoading}) onRefresh) async {
  final root = context.findAncestorWidgetOfExactType<TraceShell>();
  await Navigator.push(context, MaterialPageRoute(builder: (_) => PlaceDetailsScreen(api: root!.api, initialPlace: place, onRefresh: onRefresh)));
}

class PlaceDetailsScreen extends StatefulWidget {
  const PlaceDetailsScreen({super.key, required this.api, required this.initialPlace, required this.onRefresh});
  final ApiClient api;
  final TracePlace initialPlace;
  final Future<void> Function({bool showLoading}) onRefresh;
  @override
  State<PlaceDetailsScreen> createState() => _PlaceDetailsScreenState();
}

class _PlaceDetailsScreenState extends State<PlaceDetailsScreen> {
  late TracePlace place;
  bool busy = false;
  PlaceSocial? social;
  List<PlacePhoto> photos = [];
  final commentController = TextEditingController();
  final ImagePicker imagePicker = ImagePicker();

  @override
  void initState() {
    super.initState();
    place = widget.initialPlace;
    _reload();
  }

  @override
  void dispose() {
    commentController.dispose();
    super.dispose();
  }

  Future<void> _reload() async {
    try {
      final fresh = await widget.api.getPlace(place.id);
      final freshSocial = await widget.api.getPlaceSocial(place.id);
      final freshPhotos = await widget.api.getPlacePhotos(place.id);
      setState(() {
        place = fresh;
        social = freshSocial;
        photos = freshPhotos;
      });
    } catch (_) {}
  }

  Future<void> _action(Future<void> Function() call, String message) async {
    setState(() => busy = true);
    try {
      await call();
      await _reload();
      await widget.onRefresh(showLoading: false);
      if (mounted) showMessage(context, message);
    } catch (e) {
      if (mounted) showMessage(context, '처리 실패: $e');
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> _toggleLike() async {
    if (!widget.api.isSignedIn) {
      showMessage(context, '좋아요는 로그인 후 사용할 수 있습니다.');
      return;
    }
    await _action(() => widget.api.toggleLike(place.id).then((value) => social = value), '좋아요 상태를 변경했습니다.');
  }

  Future<void> _addComment() async {
    if (!widget.api.isSignedIn) {
      showMessage(context, '댓글은 로그인 후 작성할 수 있습니다.');
      return;
    }
    final content = commentController.text.trim();
    if (content.isEmpty) {
      showMessage(context, '댓글 내용을 입력하세요.');
      return;
    }
    await _action(() async {
      social = await widget.api.addComment(place.id, content);
      commentController.clear();
    }, '댓글을 등록했습니다.');
  }

  Future<void> _uploadPhotos() async {
    if (!widget.api.isSignedIn) {
      showMessage(context, '사진 업로드는 로그인 후 사용할 수 있습니다.');
      return;
    }

    final picked = await imagePicker.pickMultiImage(imageQuality: 85);
    if (picked.isEmpty) return;

    setState(() => busy = true);
    try {
      await widget.api.uploadPlacePhotos(place.id, picked);
      await _reload();
      await widget.onRefresh(showLoading: false);
      if (mounted) showMessage(context, '사진을 업로드했습니다.');
    } catch (e) {
      if (mounted) showMessage(context, '사진 업로드 실패: $e');
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> _deletePhoto(PlacePhoto photo) async {
    if (!widget.api.isSignedIn) {
      showMessage(context, '사진 삭제는 로그인 후 사용할 수 있습니다.');
      return;
    }
    final ok = await confirm(context, '이 사진을 삭제하시겠습니까?');
    if (ok != true) return;

    setState(() => busy = true);
    try {
      await widget.api.deletePlacePhoto(place.id, photo.id);
      await _reload();
      await widget.onRefresh(showLoading: false);
      if (mounted) showMessage(context, '사진을 삭제했습니다.');
    } catch (e) {
      if (mounted) showMessage(context, '사진 삭제 실패: $e');
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('장소 상세 정보')),
        body: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(place.name, style: const TextStyle(fontSize: 26, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 12),
                  DetailRow(icon: Icons.label, label: '카테고리', value: place.category),
                  DetailRow(icon: Icons.description, label: '설명', value: place.description),
                  DetailRow(icon: Icons.directions_run, label: '추천 활동', value: place.recommendedActivities),
                  DetailRow(icon: Icons.event_available, label: '방문 여부', value: place.isVisited ? '완료' : '예정'),
                  DetailRow(icon: Icons.bar_chart, label: '방문 횟수', value: '${place.visitCount}회'),
                  DetailRow(icon: Icons.map, label: '위치', value: '${place.latitude}, ${place.longitude}'),
                  DetailRow(icon: Icons.star, label: '공유 여부', value: place.isShared ? '공유 가능' : '개인 기록'),
                  DetailRow(icon: Icons.photo_library, label: '사진', value: '${place.photoCount}장'),
                  if (place.sharedDescription.isNotEmpty) DetailRow(icon: Icons.share, label: '공유 설명', value: place.sharedDescription),
                ]),
              ),
            ),
            const SizedBox(height: 8),
            if (busy) const LinearProgressIndicator(),
            if (!place.isVisited)
              FilledButton(onPressed: busy ? null : () => _action(() => widget.api.markVisited(place.id), '방문 완료로 변경했습니다.'), child: const Text('방문 완료로 변경'))
            else ...[
              FilledButton(onPressed: busy ? null : () => _action(() => widget.api.addVisit(place.id), '방문 횟수를 추가했습니다.'), child: const Text('방문 횟수 +1')),
              OutlinedButton(onPressed: busy ? null : () => _action(() => widget.api.removeVisit(place.id), '방문 횟수를 차감했습니다.'), child: const Text('방문 횟수 -1')),
            ],
            Row(children: [
              Expanded(child: OutlinedButton.icon(
                onPressed: busy ? null : () async {
                  await Navigator.push(context, MaterialPageRoute(builder: (_) => PlaceFormScreen(api: widget.api, existing: place, onSaved: widget.onRefresh)));
                  await _reload();
                },
                icon: const Icon(Icons.edit), label: const Text('수정하기'),
              )),
              const SizedBox(width: 8),
              Expanded(child: FilledButton.icon(
                style: FilledButton.styleFrom(backgroundColor: Colors.red),
                onPressed: busy ? null : () async {
                  final ok = await confirm(context, '정말 삭제하시겠습니까? 목록과 지도에서 함께 제거됩니다.');
                  if (ok != true) return;
                  await _action(() => widget.api.deletePlace(place.id), '장소를 삭제했습니다.');
                  if (mounted) Navigator.pop(context);
                },
                icon: const Icon(Icons.delete), label: const Text('삭제하기'),
              )),
            ]),
            const SizedBox(height: 16),
            PhotoSection(
              api: widget.api,
              photos: photos,
              isSignedIn: widget.api.isSignedIn,
              canModify: place.canModify,
              busy: busy,
              onUpload: _uploadPhotos,
              onDelete: _deletePhoto,
            ),
            const SizedBox(height: 16),
            SocialSection(
              social: social,
              isSignedIn: widget.api.isSignedIn,
              busy: busy,
              commentController: commentController,
              onLike: _toggleLike,
              onComment: _addComment,
            ),
          ],
        ),
      );
}


class PhotoSection extends StatelessWidget {
  const PhotoSection({
    super.key,
    required this.api,
    required this.photos,
    required this.isSignedIn,
    required this.canModify,
    required this.busy,
    required this.onUpload,
    required this.onDelete,
  });

  final ApiClient api;
  final List<PlacePhoto> photos;
  final bool isSignedIn;
  final bool canModify;
  final bool busy;
  final VoidCallback onUpload;
  final ValueChanged<PlacePhoto> onDelete;

  @override
  Widget build(BuildContext context) => Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Expanded(child: Text('장소 사진 갤러리', style: Theme.of(context).textTheme.titleLarge)),
              FilledButton.icon(
                onPressed: canModify && !busy ? onUpload : null,
                icon: const Icon(Icons.add_photo_alternate),
                label: const Text('사진 업로드'),
              ),
            ]),
            const SizedBox(height: 6),
            Text(
              !isSignedIn
                  ? '사진 업로드는 로그인한 사용자만 사용할 수 있습니다.'
                  : canModify
                      ? '사진은 앱 서버의 뷰어 API를 통해 표시됩니다. Blob URL은 직접 노출하지 않습니다.'
                      : '사진 업로드와 삭제는 이 장소를 등록한 사용자만 사용할 수 있습니다.',
              style: const TextStyle(color: Colors.black54),
            ),
            const SizedBox(height: 12),
            if (photos.isEmpty)
              const Text('아직 등록된 사진이 없습니다.')
            else
              GridView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: photos.length,
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  mainAxisSpacing: 8,
                  crossAxisSpacing: 8,
                  childAspectRatio: .85,
                ),
                itemBuilder: (context, index) {
                  final photo = photos[index];
                  return ClipRRect(
                    borderRadius: BorderRadius.circular(16),
                    child: Stack(children: [
                      Positioned.fill(
                        child: Image.network(
                          api.absoluteUrl(photo.viewerUrl),
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => const ColoredBox(
                            color: Color(0xffe2e8f0),
                            child: Center(child: Icon(Icons.broken_image)),
                          ),
                        ),
                      ),
                      Positioned(
                        left: 0,
                        right: 0,
                        bottom: 0,
                        child: Container(
                          padding: const EdgeInsets.all(8),
                          color: Colors.black54,
                          child: Text(
                            '${photo.storageProvider} · ${(photo.size / 1024).ceil()}KB',
                            style: const TextStyle(color: Colors.white, fontSize: 12),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ),
                      if (canModify)
                        Positioned(
                          top: 4,
                          right: 4,
                          child: IconButton.filledTonal(
                            tooltip: '사진 삭제',
                            onPressed: busy ? null : () => onDelete(photo),
                            icon: const Icon(Icons.delete),
                          ),
                        ),
                    ]),
                  );
                },
              ),
          ]),
        ),
      );
}

class SocialSection extends StatelessWidget {
  const SocialSection({
    super.key,
    required this.social,
    required this.isSignedIn,
    required this.busy,
    required this.commentController,
    required this.onLike,
    required this.onComment,
  });

  final PlaceSocial? social;
  final bool isSignedIn;
  final bool busy;
  final TextEditingController commentController;
  final VoidCallback onLike;
  final VoidCallback onComment;

  @override
  Widget build(BuildContext context) {
    final data = social;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            Expanded(child: Text('좋아요와 댓글', style: Theme.of(context).textTheme.titleLarge)),
            OutlinedButton.icon(
              onPressed: busy ? null : onLike,
              icon: Icon(data?.likedByMe == true ? Icons.favorite : Icons.favorite_border),
              label: Text('좋아요 ${data?.likeCount ?? 0}'),
            ),
          ]),
          if (!isSignedIn)
            const Padding(
              padding: EdgeInsets.only(top: 8, bottom: 8),
              child: Text('로그인한 사용자만 좋아요와 댓글을 사용할 수 있습니다.', style: TextStyle(color: Colors.black54)),
            ),
          TextField(
            controller: commentController,
            enabled: isSignedIn && !busy,
            minLines: 2,
            maxLines: 4,
            decoration: const InputDecoration(labelText: '댓글 입력', hintText: '이 장소에 대한 생각을 남겨 보세요.'),
          ),
          const SizedBox(height: 8),
          Align(alignment: Alignment.centerRight, child: FilledButton.icon(onPressed: isSignedIn && !busy ? onComment : null, icon: const Icon(Icons.send), label: const Text('댓글 등록'))),
          const Divider(height: 28),
          if (data == null)
            const Text('댓글을 불러오는 중입니다...')
          else if (data.comments.isEmpty)
            const Text('아직 댓글이 없습니다.')
          else
            ...data.comments.map((comment) => ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: const CircleAvatar(child: Icon(Icons.person)),
                  title: Text(comment.userName, style: const TextStyle(fontWeight: FontWeight.bold)),
                  subtitle: Text(comment.content),
                  trailing: Text('${comment.createdAt.month}/${comment.createdAt.day}'),
                )),
        ]),
      ),
    );
  }
}

class DetailRow extends StatelessWidget {
  const DetailRow({super.key, required this.icon, required this.label, required this.value});
  final IconData icon;
  final String label;
  final String value;
  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.symmetric(vertical: 7),
        child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Icon(icon, size: 22, color: Theme.of(context).colorScheme.primary),
          const SizedBox(width: 10),
          SizedBox(width: 86, child: Text(label, style: const TextStyle(fontWeight: FontWeight.bold))),
          Expanded(child: Text(value.isEmpty ? '-' : value)),
        ]),
      );
}

class PlaceFormScreen extends StatefulWidget {
  const PlaceFormScreen({super.key, required this.api, required this.onSaved, this.existing});
  final ApiClient api;
  final Future<void> Function({bool showLoading}) onSaved;
  final TracePlace? existing;
  @override
  State<PlaceFormScreen> createState() => _PlaceFormScreenState();
}

class _PlaceFormScreenState extends State<PlaceFormScreen> {
  final formKey = GlobalKey<FormState>();
  late final TextEditingController name;
  late final TextEditingController description;
  late final TextEditingController activities;
  late final TextEditingController sharedDescription;
  String category = '산책';
  bool isVisited = false;
  bool isShared = false;
  int visitCount = 0;
  double latitude = 34.9501;
  double longitude = 127.4872;
  bool busy = false;

  final categories = const ['산책', '운동 / 산책', '사진', '그래피티 / 사진', '맛집', '휴식', '문화'];

  @override
  void initState() {
    super.initState();
    final p = widget.existing;
    name = TextEditingController(text: p?.name ?? '');
    description = TextEditingController(text: p?.description ?? '');
    activities = TextEditingController(text: p?.recommendedActivities ?? '');
    sharedDescription = TextEditingController(text: p?.sharedDescription ?? '');
    category = categories.contains(p?.category) ? p!.category : (p?.category ?? '산책');
    isVisited = p?.isVisited ?? false;
    isShared = p?.isShared ?? false;
    visitCount = p?.visitCount ?? 0;
    latitude = p?.latitude == 0 || p?.latitude == null ? 34.9501 : p!.latitude;
    longitude = p?.longitude == 0 || p?.longitude == null ? 127.4872 : p!.longitude;
  }

  @override
  void dispose() {
    name.dispose();
    description.dispose();
    activities.dispose();
    sharedDescription.dispose();
    super.dispose();
  }

  Future<void> save() async {
    if (!formKey.currentState!.validate()) return;
    setState(() => busy = true);
    final p = TracePlace(
      id: widget.existing?.id ?? 0,
      name: name.text.trim(),
      category: category,
      description: description.text.trim(),
      recommendedActivities: activities.text.trim(),
      isVisited: isVisited,
      visitCount: isVisited ? (visitCount <= 0 ? 1 : visitCount) : 0,
      latitude: latitude,
      longitude: longitude,
      isShared: isShared,
      sharedDescription: sharedDescription.text.trim(),
      // 더 이상 화면에 표시하지 않는 확장용 값은 수정 시 기존 값을 유지합니다.
      photoUrl: widget.existing?.photoUrl,
    );
    try {
      if (widget.existing == null) {
        await widget.api.createPlace(p);
      } else {
        await widget.api.updatePlace(p);
      }
      await widget.onSaved(showLoading: false);
      if (mounted) {
        showMessage(context, widget.existing == null ? '새 장소를 추가했습니다.' : '장소 정보를 수정했습니다.');
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) showMessage(context, '저장 실패: $e');
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: Text(widget.existing == null ? '새 장소 추가' : '장소 수정')),
        body: Form(
          key: formKey,
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              TextFormField(controller: name, decoration: const InputDecoration(labelText: '장소 이름'), validator: (v) => v == null || v.trim().isEmpty ? '장소 이름을 입력하세요.' : null),
              const SizedBox(height: 10),
              DropdownButtonFormField<String>(
                value: categories.contains(category) ? category : null,
                decoration: const InputDecoration(labelText: '카테고리'),
                items: categories.map((c) => DropdownMenuItem(value: c, child: Text(c))).toList(),
                onChanged: (v) => setState(() => category = v ?? '산책'),
              ),
              const SizedBox(height: 10),
              TextFormField(controller: description, minLines: 3, maxLines: 5, decoration: const InputDecoration(labelText: '설명')),
              const SizedBox(height: 10),
              TextFormField(controller: activities, decoration: const InputDecoration(labelText: '추천 활동')),
              SwitchListTile(title: const Text('이미 방문한 장소로 기록'), value: isVisited, onChanged: (v) => setState(() { isVisited = v; if (!v) visitCount = 0; if (v && visitCount == 0) visitCount = 1; })),
              TextFormField(
                initialValue: visitCount.toString(),
                enabled: isVisited,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(labelText: '방문 횟수'),
                onChanged: (v) => visitCount = int.tryParse(v) ?? visitCount,
              ),
              SwitchListTile(title: const Text('추천 스팟에 공유 가능'), value: isShared, onChanged: (v) => setState(() => isShared = v)),
              TextFormField(controller: sharedDescription, minLines: 2, maxLines: 4, decoration: const InputDecoration(labelText: '공유용 설명')),
              const SizedBox(height: 14),
              Card(child: ListTile(
                leading: const Icon(Icons.my_location),
                title: const Text('위치 정보'),
                subtitle: Text('위도 $latitude / 경도 $longitude'),
                trailing: const Icon(Icons.chevron_right),
                onTap: () async {
                  final picked = await Navigator.push<LatLng>(context, MaterialPageRoute(builder: (_) => LocationPickerScreen(initial: LatLng(latitude, longitude))));
                  if (picked != null) setState(() { latitude = picked.latitude; longitude = picked.longitude; });
                },
              )),
              const SizedBox(height: 12),
              if (busy) const LinearProgressIndicator(),
              FilledButton.icon(onPressed: busy ? null : save, icon: const Icon(Icons.save), label: const Text('저장하기')),
            ],
          ),
        ),
      );
}

class LocationPickerScreen extends StatefulWidget {
  const LocationPickerScreen({super.key, required this.initial});
  final LatLng initial;
  @override
  State<LocationPickerScreen> createState() => _LocationPickerScreenState();
}

class _LocationPickerScreenState extends State<LocationPickerScreen> {
  late LatLng selected;
  @override
  void initState() {
    super.initState();
    selected = widget.initial;
  }

  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('지도에서 위치 선택')),
        body: Column(children: [
          Expanded(child: FlutterMap(
            options: MapOptions(initialCenter: selected, initialZoom: 14, onTap: (_, point) => setState(() => selected = point)),
            children: [
              TileLayer(urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png', userAgentPackageName: 'com.example.trace_map'),
              MarkerLayer(markers: [Marker(point: selected, width: 48, height: 48, child: const Icon(Icons.location_on, size: 48, color: Colors.red))]),
            ],
          )),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(children: [
              Text('선택 위치: ${selected.latitude.toStringAsFixed(6)}, ${selected.longitude.toStringAsFixed(6)}'),
              const SizedBox(height: 8),
              FilledButton(onPressed: () => Navigator.pop(context, selected), child: const Text('이 위치로 설정')),
            ]),
          ),
        ]),
      );
}

class ChallengesScreen extends StatelessWidget {
  const ChallengesScreen({super.key, required this.challenges, required this.isSignedIn, required this.onRefresh});
  final List<ChallengeStatus> challenges;
  final bool isSignedIn;
  final Future<void> Function({bool showLoading}) onRefresh;
  @override
  Widget build(BuildContext context) => ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const ScreenHeader(title: '도전과제', subtitle: '로그인한 계정의 장소 추가와 방문 기록 기준으로 계산됩니다.'),
          if (!isSignedIn)
            const InfoCard(title: '로그인 필요', body: '도전과제와 기록 장소 카운트는 로그인 후 본인 계정 기준으로 제공됩니다.')
          else
            ...challenges.map((c) => Card(child: ListTile(
            leading: Icon(c.isCompleted ? Icons.check_box : Icons.check_box_outline_blank, color: c.isCompleted ? Theme.of(context).colorScheme.primary : null),
            title: Text(c.title, style: const TextStyle(fontWeight: FontWeight.bold)),
            subtitle: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(c.description),
              const SizedBox(height: 6),
              LinearProgressIndicator(value: c.target == 0 ? 0 : c.current / c.target),
              Text('${c.current} / ${c.target}'),
            ]),
          ))),
        ],
      );
}

class RecommendationsScreen extends StatelessWidget {
  const RecommendationsScreen({super.key, required this.places, required this.onRefresh});
  final List<TracePlace> places;
  final Future<void> Function({bool showLoading}) onRefresh;
  @override
  Widget build(BuildContext context) => ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const ScreenHeader(title: '추천 스팟', subtitle: '추천 스팟에 공유된 장소만 다른 사용자와 비회원에게 공개됩니다.'),
          if (places.isEmpty)
            const InfoCard(title: '추천 스팟 없음', body: '장소를 추가하거나 수정할 때 추천 스팟 공유를 켜면 이 화면에 표시됩니다.')
          else
            ...places.map((p) => PlaceSummaryCard(place: p, onTap: () => openDetails(context, p, onRefresh))),
        ],
      );
}

class ScreenHeader extends StatelessWidget {
  const ScreenHeader({super.key, required this.title, required this.subtitle});
  final String title;
  final String subtitle;
  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(title, style: const TextStyle(fontSize: 28, fontWeight: FontWeight.w900)),
          const SizedBox(height: 4),
          Text(subtitle, style: const TextStyle(color: Colors.black54)),
        ]),
      );
}

class AuthScreen extends StatefulWidget {
  const AuthScreen({super.key, required this.api, required this.onLogin});
  final ApiClient api;
  final Future<void> Function(AuthUser? user) onLogin;
  @override
  State<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends State<AuthScreen> {
  final email = TextEditingController();
  final password = TextEditingController();
  final displayName = TextEditingController();
  bool registerMode = false;
  bool busy = false;
  PlaceSocial? social;
  List<PlacePhoto> photos = [];
  final commentController = TextEditingController();
  final ImagePicker imagePicker = ImagePicker();

  @override
  void dispose() {
    email.dispose();
    password.dispose();
    displayName.dispose();
    super.dispose();
  }

  Future<void> submit() async {
    if (email.text.trim().isEmpty || password.text.isEmpty) {
      showMessage(context, '이메일과 비밀번호를 입력하세요.');
      return;
    }
    setState(() => busy = true);
    try {
      if (registerMode) {
        await widget.api.register(email.text.trim(), password.text, displayName.text.trim());
      }
      final me = await widget.api.login(email.text.trim(), password.text);
      await widget.onLogin(me);
      if (mounted) {
        showMessage(context, registerMode ? '회원가입 후 로그인되었습니다.' : '로그인되었습니다.');
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) showMessage(context, '인증 실패: $e');
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: Text(registerMode ? '회원가입' : '로그인')),
        body: ListView(padding: const EdgeInsets.all(20), children: [
          const InfoCard(title: '라이브 인증 API', body: 'https://tracemap.azurewebsites.net/api/identity/register, /login 및 /api/auth/me를 호출합니다.'),
          const SizedBox(height: 14),
          if (registerMode) TextField(controller: displayName, decoration: const InputDecoration(labelText: '표시 이름')),
          TextField(controller: email, keyboardType: TextInputType.emailAddress, decoration: const InputDecoration(labelText: '이메일')),
          TextField(controller: password, obscureText: true, decoration: const InputDecoration(labelText: '비밀번호')),
          const SizedBox(height: 14),
          if (busy) const LinearProgressIndicator(),
          FilledButton(onPressed: busy ? null : submit, child: Text(registerMode ? '회원가입 후 로그인' : '로그인')),
          TextButton(onPressed: busy ? null : () => setState(() => registerMode = !registerMode), child: Text(registerMode ? '이미 계정이 있습니다. 로그인' : '계정이 없습니다. 회원가입')),
        ]),
      );
}

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key, required this.user, required this.onLogout});
  final AuthUser user;
  final Future<void> Function() onLogout;
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('회원정보')),
        body: ListView(padding: const EdgeInsets.all(20), children: [
          InfoCard(title: user.displayName, body: '이메일: ${user.email}\n사용자명: ${user.userName}'),
          const SizedBox(height: 16),
          FilledButton.icon(
            onPressed: () async {
              await onLogout();
              if (context.mounted) Navigator.pop(context);
            },
            icon: const Icon(Icons.logout),
            label: const Text('로그아웃'),
          ),
        ]),
      );
}

class ApiClient {
  ApiClient(this.baseUrl);
  final String baseUrl;
  final http.Client _http = http.Client();
  String? accessToken;
  String? refreshToken;

  bool get isSignedIn => accessToken != null && accessToken!.isNotEmpty;

  Future<void> loadSavedToken() async {
    final prefs = await SharedPreferences.getInstance();
    accessToken = prefs.getString('accessToken');
    refreshToken = prefs.getString('refreshToken');
  }

  Future<void> saveToken(String token, String? refresh) async {
    accessToken = token;
    refreshToken = refresh;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('accessToken', token);
    if (refresh != null) await prefs.setString('refreshToken', refresh);
  }

  Future<void> clearToken() async {
    accessToken = null;
    refreshToken = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('accessToken');
    await prefs.remove('refreshToken');
  }

  Map<String, String> get headers => {
    'Content-Type': 'application/json',
    if (accessToken != null) 'Authorization': 'Bearer $accessToken',
  };

  Uri uri(String path, [Map<String, String>? query]) => Uri.parse('$baseUrl$path').replace(queryParameters: query);
  String absoluteUrl(String value) => value.startsWith('http') ? value : '$baseUrl$value';

  Future<void> register(String email, String password, String displayName) async {
    final body = {'email': email, 'password': password, if (displayName.isNotEmpty) 'displayName': displayName};
    final response = await _http.post(uri('/api/identity/register'), headers: headers, body: jsonEncode(body));
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
  }

  Future<AuthUser?> login(String email, String password) async {
    final response = await _http.post(
      uri('/api/identity/login', {'useCookies': 'false', 'useSessionCookies': 'false'}),
      headers: headers,
      body: jsonEncode({'email': email, 'password': password}),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    final json = jsonDecode(response.body) as Map<String, dynamic>;
    final token = json['accessToken']?.toString() ?? json['token']?.toString();
    if (token == null || token.isEmpty) throw Exception('로그인 응답에서 accessToken을 찾지 못했습니다.');
    await saveToken(token, json['refreshToken']?.toString());
    return getMe();
  }

  Future<AuthUser> getMe() async {
    final response = await _http.get(uri('/api/auth/me'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    return AuthUser.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<List<TracePlace>> getPlaces() async {
    if (!isSignedIn) return [];

    final response = await _http.get(uri('/api/places'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => TracePlace.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<List<TracePlace>> getSharedPlaces() async {
    final response = await _http.get(uri('/api/places/shared'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => TracePlace.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<TracePlace> getPlace(int id) async {
    final response = await _http.get(uri('/api/places/$id'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    return TracePlace.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<TracePlace> createPlace(TracePlace place) async {
    final response = await _http.post(uri('/api/places'), headers: headers, body: jsonEncode(place.toJson()));
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    return TracePlace.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<void> updatePlace(TracePlace place) async {
    final response = await _http.put(uri('/api/places/${place.id}'), headers: headers, body: jsonEncode(place.toJson()));
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
  }

  Future<void> deletePlace(int id) async {
    final response = await _http.delete(uri('/api/places/$id'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
  }

  Future<void> markVisited(int id) => postNoContent('/api/places/$id/mark-visited');
  Future<void> addVisit(int id) => postNoContent('/api/places/$id/visit-plus');
  Future<void> removeVisit(int id) => postNoContent('/api/places/$id/visit-minus');

  Future<void> postNoContent(String path) async {
    final response = await _http.post(uri(path), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
  }

  Future<PlaceSocial> getPlaceSocial(int placeId) async {
    final response = await _http.get(uri('/api/places/$placeId/social'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    return PlaceSocial.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<PlaceSocial> toggleLike(int placeId) async {
    final response = await _http.post(uri('/api/places/$placeId/like'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    return PlaceSocial.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<PlaceSocial> addComment(int placeId, String content) async {
    final response = await _http.post(uri('/api/places/$placeId/comments'), headers: headers, body: jsonEncode({'content': content}));
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    return PlaceSocial.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<List<PlacePhoto>> getPlacePhotos(int placeId) async {
    final response = await _http.get(uri('/api/places/$placeId/photos'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => PlacePhoto.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<List<PlacePhoto>> uploadPlacePhotos(int placeId, List<XFile> files) async {
    final request = http.MultipartRequest('POST', uri('/api/places/$placeId/photos'));
    request.headers['Accept'] = 'application/json';
    if (accessToken != null) request.headers['Authorization'] = 'Bearer $accessToken';

    for (final file in files) {
      final bytes = await file.readAsBytes();
      if (bytes.isEmpty) continue;

      final contentType = resolveImageContentType(
        fileName: file.name,
        mimeType: file.mimeType,
        bytes: bytes,
      );
      final uploadFileName = safeUploadImageFileName(file.name, contentType);

      request.files.add(
        http.MultipartFile.fromBytes(
          'files',
          bytes,
          filename: uploadFileName,
          contentType: MediaType.parse(contentType),
        ),
      );
    }

    if (request.files.isEmpty) throw Exception('업로드할 사진을 찾지 못했습니다.');

    final streamed = await _http.send(request);
    final response = await http.Response.fromStream(streamed);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => PlacePhoto.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<void> deletePlacePhoto(int placeId, int photoId) async {
    final response = await _http.delete(uri('/api/places/$placeId/photos/$photoId'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
  }

  Future<List<ChallengeStatus>> getChallenges() async {
    if (!isSignedIn) return [];

    final response = await _http.get(uri('/api/challenges'), headers: headers);
    if (response.statusCode < 200 || response.statusCode >= 300) throw ApiException.fromResponse(response);
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => ChallengeStatus.fromJson(e as Map<String, dynamic>)).toList();
  }
}


String resolveImageContentType({
  required String fileName,
  required String? mimeType,
  required List<int> bytes,
}) {
  final supplied = mimeType?.trim().toLowerCase();
  if (supplied != null && supplied.startsWith('image/')) {
    if (supplied == 'image/jpg') return 'image/jpeg';
    return supplied;
  }

  final extension = imageExtensionFromName(fileName);
  final fromExtension = contentTypeFromExtension(extension);
  if (fromExtension != null) return fromExtension;

  final fromBytes = contentTypeFromImageBytes(bytes);
  if (fromBytes != null) return fromBytes;

  // image_picker가 이미지만 반환하므로, 일부 Android 기기에서 MIME 정보와 확장자가
  // 누락되는 경우에도 서버가 application/octet-stream으로 오인하지 않도록 기본값을 지정한다.
  return 'image/jpeg';
}

String safeUploadImageFileName(String originalName, String contentType) {
  final rawName = originalName.trim().split(RegExp(r'[\/]')).last;
  final baseName = rawName.isEmpty ? 'photo' : rawName;
  final extension = imageExtensionFromName(baseName);

  if (contentTypeFromExtension(extension) != null) {
    return baseName;
  }

  final dotIndex = baseName.lastIndexOf('.');
  final nameOnly = dotIndex > 0 ? baseName.substring(0, dotIndex) : baseName;
  final safeName = nameOnly
      .replaceAll(RegExp(r'[^A-Za-z0-9._-]'), '_')
      .replaceAll(RegExp(r'_+'), '_')
      .replaceAll(RegExp(r'^_+|_+$'), '');
  return '${safeName.isEmpty ? 'photo' : safeName}.${extensionFromContentType(contentType)}';
}

String imageExtensionFromName(String fileName) {
  final cleanName = fileName.trim().split(RegExp(r'[\/]')).last;
  final dotIndex = cleanName.lastIndexOf('.');
  if (dotIndex < 0 || dotIndex == cleanName.length - 1) return '';
  return cleanName.substring(dotIndex).toLowerCase();
}

String? contentTypeFromExtension(String extension) => switch (extension.toLowerCase()) {
      '.jpg' || '.jpeg' => 'image/jpeg',
      '.png' => 'image/png',
      '.webp' => 'image/webp',
      '.gif' => 'image/gif',
      '.heic' => 'image/heic',
      '.heif' => 'image/heif',
      _ => null,
    };

String extensionFromContentType(String contentType) => switch (contentType.toLowerCase()) {
      'image/png' => 'png',
      'image/webp' => 'webp',
      'image/gif' => 'gif',
      'image/heic' => 'heic',
      'image/heif' => 'heif',
      _ => 'jpg',
    };

String? contentTypeFromImageBytes(List<int> bytes) {
  if (bytes.length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) {
    return 'image/jpeg';
  }
  if (bytes.length >= 8 &&
      bytes[0] == 0x89 &&
      bytes[1] == 0x50 &&
      bytes[2] == 0x4e &&
      bytes[3] == 0x47 &&
      bytes[4] == 0x0d &&
      bytes[5] == 0x0a &&
      bytes[6] == 0x1a &&
      bytes[7] == 0x0a) {
    return 'image/png';
  }
  if (bytes.length >= 6 &&
      bytes[0] == 0x47 &&
      bytes[1] == 0x49 &&
      bytes[2] == 0x46 &&
      bytes[3] == 0x38 &&
      (bytes[4] == 0x37 || bytes[4] == 0x39) &&
      bytes[5] == 0x61) {
    return 'image/gif';
  }
  if (bytes.length >= 12 &&
      bytes[0] == 0x52 &&
      bytes[1] == 0x49 &&
      bytes[2] == 0x46 &&
      bytes[3] == 0x46 &&
      bytes[8] == 0x57 &&
      bytes[9] == 0x45 &&
      bytes[10] == 0x42 &&
      bytes[11] == 0x50) {
    return 'image/webp';
  }
  if (bytes.length >= 12 &&
      bytes[4] == 0x66 &&
      bytes[5] == 0x74 &&
      bytes[6] == 0x79 &&
      bytes[7] == 0x70) {
    final brand = String.fromCharCodes(bytes.sublist(8, 12)).toLowerCase();
    if (brand.startsWith('hei') || brand.startsWith('heic') || brand.startsWith('heix') || brand.startsWith('hevc') || brand.startsWith('hevx')) {
      return brand.contains('f') ? 'image/heif' : 'image/heic';
    }
  }
  return null;
}

class ApiException implements Exception {
  ApiException(this.message);
  final String message;
  factory ApiException.fromResponse(http.Response response) {
    var message = response.body;
    try {
      final parsed = jsonDecode(response.body);
      message = parsed.toString();
    } catch (_) {}
    return ApiException('HTTP ${response.statusCode}: $message');
  }
  @override
  String toString() => message;
}

class AuthUser {
  const AuthUser({required this.isAuthenticated, required this.userName, required this.email, required this.displayName});
  final bool isAuthenticated;
  final String userName;
  final String email;
  final String displayName;
  factory AuthUser.fromJson(Map<String, dynamic> json) => AuthUser(
    isAuthenticated: json['isAuthenticated'] == true,
    userName: (json['userName'] ?? '').toString(),
    email: (json['email'] ?? '').toString(),
    displayName: ((json['userName'] ?? json['email'] ?? 'TraceMap 사용자').toString()),
  );
}

class TracePlace {
  const TracePlace({
    required this.id,
    required this.name,
    required this.category,
    required this.description,
    required this.recommendedActivities,
    required this.isVisited,
    required this.visitCount,
    required this.latitude,
    required this.longitude,
    required this.isShared,
    required this.sharedDescription,
    this.photoUrl,
    this.likeCount = 0,
    this.commentCount = 0,
    this.photoCount = 0,
  });

  final int id;
  final String name;
  final String category;
  final String description;
  final String recommendedActivities;
  final bool isVisited;
  final int visitCount;
  final double latitude;
  final double longitude;
  final bool isShared;
  final String sharedDescription;
  final String? photoUrl;
  final int likeCount;
  final int commentCount;
  final int photoCount;

  factory TracePlace.fromJson(Map<String, dynamic> json) => TracePlace(
    id: asInt(json, 'id'),
    name: asString(json, 'name'),
    category: asString(json, 'category', fallback: '산책'),
    description: asString(json, 'description'),
    recommendedActivities: asString(json, 'recommendedActivities'),
    isVisited: asBool(json, 'isVisited'),
    visitCount: asInt(json, 'visitCount'),
    latitude: asDouble(json, 'latitude'),
    longitude: asDouble(json, 'longitude'),
    isShared: asBool(json, 'isShared'),
    sharedDescription: asString(json, 'sharedDescription'),
    photoUrl: asNullableString(json, 'photoUrl'),
    likeCount: asInt(json, 'likeCount'),
    commentCount: asInt(json, 'commentCount'),
    photoCount: asInt(json, 'photoCount'),
  );

  Map<String, dynamic> toJson() => {
    'id': id,
    'name': name,
    'category': category,
    'description': description,
    'recommendedActivities': recommendedActivities,
    'isVisited': isVisited,
    'visitCount': visitCount,
    'latitude': latitude,
    'longitude': longitude,
    'isShared': isShared,
    'sharedDescription': sharedDescription,
    'photoUrl': photoUrl,
    'likeCount': likeCount,
    'commentCount': commentCount,
    'photoCount': photoCount,
  };
}


class PlacePhoto {
  const PlacePhoto({
    required this.id,
    required this.tracePlaceId,
    required this.fileName,
    required this.contentType,
    required this.size,
    required this.storageProvider,
    required this.viewerUrl,
    required this.userName,
    required this.isAnonymous,
    required this.createdAt,
    required this.updatedAt,
  });

  final int id;
  final int tracePlaceId;
  final String fileName;
  final String contentType;
  final int size;
  final String storageProvider;
  final String viewerUrl;
  final String userName;
  final bool isAnonymous;
  final DateTime createdAt;
  final DateTime updatedAt;

  factory PlacePhoto.fromJson(Map<String, dynamic> json) => PlacePhoto(
    id: asInt(json, 'id'),
    tracePlaceId: asInt(json, 'tracePlaceId'),
    fileName: asString(json, 'fileName'),
    contentType: asString(json, 'contentType', fallback: 'image/jpeg'),
    size: asInt(json, 'size'),
    storageProvider: asString(json, 'storageProvider', fallback: 'Local'),
    viewerUrl: asString(json, 'viewerUrl'),
    userName: asString(json, 'userName', fallback: 'TraceMap 사용자'),
    isAnonymous: asBool(json, 'isAnonymous'),
    createdAt: DateTime.tryParse(asString(json, 'createdAt')) ?? DateTime.now(),
    updatedAt: DateTime.tryParse(asString(json, 'updatedAt')) ?? DateTime.now(),
  );
}

class PlaceSocial {
  const PlaceSocial({required this.placeId, required this.likeCount, required this.likedByMe, required this.comments});
  final int placeId;
  final int likeCount;
  final bool likedByMe;
  final List<PlaceComment> comments;

  factory PlaceSocial.fromJson(Map<String, dynamic> json) => PlaceSocial(
    placeId: asInt(json, 'placeId'),
    likeCount: asInt(json, 'likeCount'),
    likedByMe: asBool(json, 'likedByMe'),
    comments: ((json['comments'] ?? json['Comments'] ?? []) as List<dynamic>)
        .map((e) => PlaceComment.fromJson(e as Map<String, dynamic>))
        .toList(),
  );
}

class PlaceComment {
  const PlaceComment({required this.id, required this.tracePlaceId, required this.content, required this.userName, required this.isAnonymous, required this.createdAt});
  final int id;
  final int tracePlaceId;
  final String content;
  final String userName;
  final bool isAnonymous;
  final DateTime createdAt;

  factory PlaceComment.fromJson(Map<String, dynamic> json) => PlaceComment(
    id: asInt(json, 'id'),
    tracePlaceId: asInt(json, 'tracePlaceId'),
    content: asString(json, 'content'),
    userName: asString(json, 'userName', fallback: 'TraceMap 사용자'),
    isAnonymous: asBool(json, 'isAnonymous'),
    createdAt: DateTime.tryParse(asString(json, 'createdAt')) ?? DateTime.now(),
  );
}

class ChallengeStatus {
  const ChallengeStatus({required this.key, required this.title, required this.description, required this.isCompleted, required this.current, required this.target});
  final String key;
  final String title;
  final String description;
  final bool isCompleted;
  final int current;
  final int target;
  factory ChallengeStatus.fromJson(Map<String, dynamic> json) => ChallengeStatus(
    key: asString(json, 'key'),
    title: asString(json, 'title'),
    description: asString(json, 'description'),
    isCompleted: asBool(json, 'isCompleted'),
    current: asInt(json, 'current'),
    target: asInt(json, 'target'),
  );
}

String asString(Map<String, dynamic> json, String key, {String fallback = ''}) => (json[key] ?? json[_pascal(key)] ?? fallback).toString();
String? asNullableString(Map<String, dynamic> json, String key) => (json[key] ?? json[_pascal(key)])?.toString();
int asInt(Map<String, dynamic> json, String key) {
  final value = json[key] ?? json[_pascal(key)];
  if (value is int) return value;
  return int.tryParse(value?.toString() ?? '') ?? 0;
}
double asDouble(Map<String, dynamic> json, String key) {
  final value = json[key] ?? json[_pascal(key)];
  if (value is num) return value.toDouble();
  return double.tryParse(value?.toString() ?? '') ?? 0;
}
bool asBool(Map<String, dynamic> json, String key) {
  final value = json[key] ?? json[_pascal(key)];
  if (value is bool) return value;
  return value?.toString().toLowerCase() == 'true';
}
String _pascal(String key) => key.isEmpty ? key : key[0].toUpperCase() + key.substring(1);

IconData categoryIcon(String category) {
  if (category.contains('운동')) return Icons.directions_run;
  if (category.contains('사진')) return Icons.photo_camera;
  if (category.contains('그래피티')) return Icons.brush;
  if (category.contains('맛집')) return Icons.restaurant;
  if (category.contains('휴식')) return Icons.park;
  if (category.contains('문화')) return Icons.account_balance;
  return Icons.directions_walk;
}

void showMessage(BuildContext context, String message) {
  ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
}

Future<bool?> confirm(BuildContext context, String message) => showDialog<bool>(
  context: context,
  builder: (_) => AlertDialog(
    title: const Text('확인'),
    content: Text(message),
    actions: [
      TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('취소')),
      FilledButton(onPressed: () => Navigator.pop(context, true), child: const Text('확인')),
    ],
  ),
);
