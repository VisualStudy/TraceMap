using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraceMap.Data;
using TraceMap.Models;

namespace TraceMap.Services;

public class SeedDataService : ISeedDataService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeedDataService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await _db.Database.EnsureCreatedAsync();

        const string email = "administrator@tracemap.com";
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "TraceMap 관리자"
            };
            await _userManager.CreateAsync(user, "Pa$$w0rd");
        }

        if (await _db.Places.AnyAsync()) return;

        _db.Places.AddRange(
            new TracePlace
            {
                Name = "순천만 국가정원",
                Category = "산책 / 사진",
                Description = "넓은 정원과 산책로가 있어 일상 속에서 걷기 좋고 사진을 찍기 좋은 장소입니다.",
                RecommendedActivities = "산책, 사진, 휴식",
                IsVisited = true,
                VisitCount = 2,
                Latitude = 34.9273,
                Longitude = 127.5105,
                IsShared = true,
                SharedDescription = "처음 TraceMap을 사용할 때 참고하기 좋은 대표 산책 스팟입니다."
            },
            new TracePlace
            {
                Name = "조례호수공원",
                Category = "운동 / 산책",
                Description = "걷기와 가벼운 운동을 하기 좋은 호수 주변 공간입니다.",
                RecommendedActivities = "조깅, 산책, 휴식",
                IsVisited = true,
                VisitCount = 1,
                Latitude = 34.9544,
                Longitude = 127.5194,
                IsShared = true,
                SharedDescription = "가볍게 운동하거나 산책하기 좋은 장소입니다."
            },
            new TracePlace
            {
                Name = "그래피티 골목",
                Category = "그래피티 / 사진",
                Description = "벽화와 그래피티를 감상하며 사진을 남기기 좋은 골목입니다.",
                RecommendedActivities = "그래피티 감상, 사진 찍기",
                IsVisited = false,
                VisitCount = 0,
                Latitude = 34.9501,
                Longitude = 127.4872,
                IsShared = false,
                SharedDescription = "개인적으로 기록해 둔 사진 스팟입니다."
            },
            new TracePlace
            {
                Name = "노을이 잘 보이는 계단",
                Category = "사진 / 휴식",
                Description = "공식 장소명은 없지만 저녁 시간에 분위기가 좋아 개인적으로 의미를 부여한 장소입니다.",
                RecommendedActivities = "노을 감상, 글쓰기, 휴식",
                IsVisited = false,
                VisitCount = 0,
                Latitude = 34.9482,
                Longitude = 127.5015,
                IsShared = true,
                SharedDescription = "이름 없는 장소도 직접 기록할 수 있다는 TraceMap의 목적을 보여주는 추천 스팟입니다."
            }
        );
        await _db.SaveChangesAsync();
    }
}
