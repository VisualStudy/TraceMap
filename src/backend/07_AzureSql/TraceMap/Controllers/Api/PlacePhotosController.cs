using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TraceMap.Data;
using TraceMap.Models;
using TraceMap.Services;

namespace TraceMap.Controllers.Api;

[ApiController]
[Route("api/places/{placeId:int}/photos")]
public class PlacePhotosController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IPlacePhotoStorageService _storage;
    private readonly ILogger<PlacePhotosController> _logger;

    public PlacePhotosController(ApplicationDbContext db, IPlacePhotoStorageService storage, ILogger<PlacePhotosController> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPhotos(int placeId)
    {
        var exists = await _db.Places.AnyAsync(place => place.Id == placeId);
        if (!exists) return NotFound();

        var photos = await _db.PlacePhotos
            .Where(photo => photo.TracePlaceId == placeId && !photo.IsDeleted)
            .OrderByDescending(photo => photo.CreatedAt)
            .Select(photo => ToDto(photo))
            .ToListAsync();

        return Ok(photos);
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Upload(int placeId, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files.Count == 0) return BadRequest(new { message = "업로드할 사진을 선택하세요." });

        var exists = await _db.Places.AnyAsync(place => place.Id == placeId, cancellationToken);
        if (!exists) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email);
        var created = new List<PlacePhotoDto>();

        foreach (var file in files)
        {
            try
            {
                var photo = await _storage.SaveAsync(placeId, file, userId, userName, cancellationToken);
                _db.PlacePhotos.Add(photo);
                created.Add(ToDto(photo));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Photo upload failed for place {PlaceId} and file {FileName}.", placeId, file.FileName);
                return BadRequest(new { message = ex.Message });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(created);
    }

    [Authorize]
    [HttpPut("{photoId:int}")]
    [RequestSizeLimit(15_000_000)]
    public async Task<IActionResult> Replace(int placeId, int photoId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var photo = await _db.PlacePhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.TracePlaceId == placeId && !p.IsDeleted, cancellationToken);
        if (photo is null) return NotFound();

        try
        {
            await _storage.ReplaceAsync(photo, file, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(ToDto(photo));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Photo replace failed for photo {PhotoId}.", photoId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{photoId:int}")]
    public async Task<IActionResult> Delete(int placeId, int photoId, CancellationToken cancellationToken)
    {
        var photo = await _db.PlacePhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.TracePlaceId == placeId && !p.IsDeleted, cancellationToken);
        if (photo is null) return NotFound();

        await _storage.DeleteAsync(photo, cancellationToken);
        _db.PlacePhotos.Remove(photo);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{photoId:int}/content")]
    public async Task<IActionResult> Content(int placeId, int photoId, CancellationToken cancellationToken)
    {
        var photo = await _db.PlacePhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.TracePlaceId == placeId && !p.IsDeleted, cancellationToken);
        if (photo is null) return NotFound();

        var result = await _storage.OpenReadAsync(photo, cancellationToken);
        if (result is null) return NotFound();

        return File(result.Content, result.ContentType);
    }

    private static PlacePhotoDto ToDto(PlacePhoto photo)
    {
        return new PlacePhotoDto(
            photo.Id,
            photo.TracePlaceId,
            photo.FileName,
            photo.ContentType,
            photo.Size,
            photo.StorageProvider,
            $"/api/places/{photo.TracePlaceId}/photos/{photo.Id}/content",
            photo.UserName ?? (photo.IsAnonymous ? "익명 사용자" : "TraceMap 사용자"),
            photo.IsAnonymous,
            photo.CreatedAt,
            photo.UpdatedAt);
    }
}

public record PlacePhotoDto(
    int Id,
    int TracePlaceId,
    string FileName,
    string ContentType,
    long Size,
    string StorageProvider,
    string ViewerUrl,
    string UserName,
    bool IsAnonymous,
    DateTime CreatedAt,
    DateTime UpdatedAt);
