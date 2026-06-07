using System.ComponentModel.DataAnnotations;

namespace TraceMap.Models;

public class PlacePhoto
{
    public int Id { get; set; }

    [Required]
    public int TracePlaceId { get; set; }

    public TracePlace? TracePlace { get; set; }

    [Required]
    [MaxLength(260)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string BlobName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string LocalRelativePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = "image/jpeg";

    public long Size { get; set; }

    [MaxLength(32)]
    public string StorageProvider { get; set; } = "Local";

    [MaxLength(500)]
    public string? BlobETag { get; set; }

    public string? UserId { get; set; }

    [MaxLength(256)]
    public string? UserName { get; set; }

    public bool IsAnonymous { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
