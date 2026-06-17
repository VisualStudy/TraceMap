using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TraceMap.Models;
using TraceMap.Services;

namespace TraceMap.Controllers.Api;

[ApiController]
[Route("api/places")]
public class PlacesController : ControllerBase
{
    private readonly ITracePlaceService _places;

    public PlacesController(ITracePlaceService places)
    {
        _places = places;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _places.GetAllAsync(CurrentUserId()));

    [HttpGet("shared")]
    public async Task<IActionResult> Shared() => Ok(await _places.GetSharedAsync(CurrentUserId()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var place = await _places.GetByIdAsync(id, CurrentUserId());
        return place is null ? NotFound() : Ok(place);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post(TracePlace place)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (string.IsNullOrWhiteSpace(CurrentUserId())) return Unauthorized();
        ApplyWriter(place);
        var created = await _places.AddAsync(place);
        created.CanModify = true;
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, TracePlace place)
    {
        if (id != place.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        return ToActionResult(await _places.UpdateAsync(place, userId));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        return ToActionResult(await _places.DeleteAsync(id, userId));
    }

    [Authorize]
    [HttpPost("{id:int}/mark-visited")]
    public async Task<IActionResult> MarkVisited(int id)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        return ToActionResult(await _places.MarkVisitedAsync(id, userId));
    }

    [Authorize]
    [HttpPost("{id:int}/visit-plus")]
    public async Task<IActionResult> AddVisit(int id)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        return ToActionResult(await _places.AddVisitAsync(id, userId));
    }

    [Authorize]
    [HttpPost("{id:int}/visit-minus")]
    public async Task<IActionResult> RemoveVisit(int id)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        return ToActionResult(await _places.RemoveVisitAsync(id, userId));
    }

    private IActionResult ToActionResult(PlaceWriteResult result)
    {
        return result switch
        {
            PlaceWriteResult.Success => NoContent(),
            PlaceWriteResult.NotFound => NotFound(),
            PlaceWriteResult.Forbidden => Forbid(),
            _ => BadRequest()
        };
    }

    private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private void ApplyWriter(TracePlace place)
    {
        var userId = CurrentUserId();
        place.UserId = userId;
        place.UserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email);
        place.IsAnonymous = string.IsNullOrWhiteSpace(userId);
    }
}
