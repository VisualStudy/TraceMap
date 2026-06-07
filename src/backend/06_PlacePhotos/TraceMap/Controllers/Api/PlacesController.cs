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
    public async Task<IActionResult> Get() => Ok(await _places.GetAllAsync());

    [HttpGet("shared")]
    public async Task<IActionResult> Shared() => Ok(await _places.GetSharedAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var place = await _places.GetByIdAsync(id);
        return place is null ? NotFound() : Ok(place);
    }

    [HttpPost]
    public async Task<IActionResult> Post(TracePlace place)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        ApplyWriter(place);
        var created = await _places.AddAsync(place);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, TracePlace place)
    {
        if (id != place.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        ApplyWriter(place);
        return await _places.UpdateAsync(place) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await _places.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpPost("{id:int}/mark-visited")]
    public async Task<IActionResult> MarkVisited(int id) => await _places.MarkVisitedAsync(id) ? NoContent() : NotFound();

    [HttpPost("{id:int}/visit-plus")]
    public async Task<IActionResult> AddVisit(int id) => await _places.AddVisitAsync(id) ? NoContent() : NotFound();

    [HttpPost("{id:int}/visit-minus")]
    public async Task<IActionResult> RemoveVisit(int id) => await _places.RemoveVisitAsync(id) ? NoContent() : NotFound();

    private void ApplyWriter(TracePlace place)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        place.UserId = userId;
        place.UserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email);
        place.IsAnonymous = string.IsNullOrWhiteSpace(userId);
    }
}
