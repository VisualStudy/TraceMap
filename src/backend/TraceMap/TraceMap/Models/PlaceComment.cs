using System.ComponentModel.DataAnnotations;

namespace TraceMap.Models;

public class PlaceComment
{
    public int Id { get; set; }

    [Required]
    public int TracePlaceId { get; set; }

    public TracePlace? TracePlace { get; set; }

    public string? UserId { get; set; }

    [MaxLength(256)]
    public string? UserName { get; set; }

    public bool IsAnonymous { get; set; }

    [Required(ErrorMessage = "댓글 내용을 입력하세요.")]
    [MaxLength(1000)]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
