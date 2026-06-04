using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TraceMap.Models;

public class TracePlace
{
    public int Id { get; set; }

    [Required(ErrorMessage = "장소 이름을 입력하세요.")]
    [Display(Name = "장소 이름")]
    public string Name { get; set; } = "";

    [Required]
    [Display(Name = "카테고리")]
    public string Category { get; set; } = "산책";

    [Display(Name = "설명")]
    public string Description { get; set; } = "";

    [Display(Name = "추천 활동")]
    public string RecommendedActivities { get; set; } = "";

    [Display(Name = "방문 완료")]
    public bool IsVisited { get; set; }

    [Display(Name = "방문 횟수")]
    public int VisitCount { get; set; }

    [Display(Name = "위도")]
    public double Latitude { get; set; }

    [Display(Name = "경도")]
    public double Longitude { get; set; }

    [Display(Name = "공유 여부")]
    public bool IsShared { get; set; }

    [Display(Name = "공유용 설명")]
    public string SharedDescription { get; set; } = "";

    [Display(Name = "사진 리뷰 URL")]
    public string? PhotoUrl { get; set; }

    [Display(Name = "작성자 UserId")]
    public string? UserId { get; set; }

    [Display(Name = "작성자 이름")]
    public string? UserName { get; set; }

    [Display(Name = "익명 작성 여부")]
    public bool IsAnonymous { get; set; } = true;

    [NotMapped]
    public int LikeCount { get; set; }

    [NotMapped]
    public int CommentCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
