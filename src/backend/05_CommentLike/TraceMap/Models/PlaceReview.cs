using System.ComponentModel.DataAnnotations;

namespace TraceMap.Models;

public class PlaceReview
{
    public int Id { get; set; }
    public int TracePlaceId { get; set; }
    public TracePlace? TracePlace { get; set; }

    [Display(Name = "후기")]
    public string Content { get; set; } = "";

    [Display(Name = "사진 URL")]
    public string? PhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
