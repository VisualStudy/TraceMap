using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TraceMap.Data;
using TraceMap.Models;

namespace TraceMap.Controllers.Api;

[ApiController]
[Route("api/places/{placeId:int}")]
public class PlaceSocialController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PlaceSocialController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("social")]
    public async Task<IActionResult> GetSocial(int placeId)
    {
        var exists = await _db.Places.AnyAsync(place => place.Id == placeId);
        if (!exists) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comments = await _db.PlaceComments
            .Where(comment => comment.TracePlaceId == placeId)
            .OrderByDescending(comment => comment.CreatedAt)
            .Select(comment => new PlaceCommentDto(
                comment.Id,
                comment.TracePlaceId,
                comment.Content,
                comment.UserName ?? (comment.IsAnonymous ? "익명 사용자" : "TraceMap 사용자"),
                comment.IsAnonymous,
                comment.CreatedAt))
            .ToListAsync();

        var likeCount = await _db.PlaceLikes.CountAsync(like => like.TracePlaceId == placeId);
        var likedByMe = userId is not null && await _db.PlaceLikes.AnyAsync(like => like.TracePlaceId == placeId && like.UserId == userId);

        return Ok(new PlaceSocialDto(placeId, likeCount, likedByMe, comments));
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> ToggleLike(int placeId)
    {
        var exists = await _db.Places.AnyAsync(place => place.Id == placeId);
        if (!exists) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var existing = await _db.PlaceLikes.FirstOrDefaultAsync(like => like.TracePlaceId == placeId && like.UserId == userId);
        if (existing is null)
        {
            _db.PlaceLikes.Add(new PlaceLike
            {
                TracePlaceId = placeId,
                UserId = userId,
                UserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email),
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            _db.PlaceLikes.Remove(existing);
        }

        await _db.SaveChangesAsync();
        return await GetSocial(placeId);
    }

    [Authorize]
    [HttpPost("comments")]
    public async Task<IActionResult> AddComment(int placeId, CreatePlaceCommentRequest request)
    {
        var content = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content)) return BadRequest(new { message = "댓글 내용을 입력하세요." });

        var exists = await _db.Places.AnyAsync(place => place.Id == placeId);
        if (!exists) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _db.PlaceComments.Add(new PlaceComment
        {
            TracePlaceId = placeId,
            UserId = userId,
            UserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email),
            IsAnonymous = false,
            Content = content,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return await GetSocial(placeId);
    }
}

public record CreatePlaceCommentRequest(string? Content);
public record PlaceCommentDto(int Id, int TracePlaceId, string Content, string UserName, bool IsAnonymous, DateTime CreatedAt);
public record PlaceSocialDto(int PlaceId, int LikeCount, bool LikedByMe, IReadOnlyList<PlaceCommentDto> Comments);
