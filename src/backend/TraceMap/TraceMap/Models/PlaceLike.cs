using System.ComponentModel.DataAnnotations;

namespace TraceMap.Models;

public class PlaceLike
{
    public int Id { get; set; }

    [Required]
    public int TracePlaceId { get; set; }

    public TracePlace? TracePlace { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    [MaxLength(256)]
    public string? UserName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
