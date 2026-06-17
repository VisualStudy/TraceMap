using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TraceMap.Models;
using TraceMap.Services;
using TraceMap.ViewModels;

namespace TraceMap.Controllers;

public class PlacesController : Controller
{
    private readonly ITracePlaceService _places;

    public PlacesController(ITracePlaceService places)
    {
        _places = places;
    }

    [Authorize]
    public async Task<IActionResult> Index(string? category)
    {
        var allPlaces = await _places.GetAllAsync(CurrentUserId());

        var categories = allPlaces
            .SelectMany(place => SplitCategories(place.Category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(categoryName => categoryName)
            .ToList();

        var filteredPlaces = string.IsNullOrWhiteSpace(category)
            ? allPlaces
            : allPlaces
                .Where(place => HasCategory(place.Category, category))
                .ToList();

        var viewModel = new PlacesIndexViewModel
        {
            Places = filteredPlaces,
            Categories = categories,
            SelectedCategory = category,
            TotalCount = allPlaces.Count
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var place = await _places.GetByIdAsync(id, CurrentUserId());

        return place is null ? NotFound() : View(place);
    }

    [Authorize]
    public IActionResult Create(double? latitude, double? longitude)
    {
        return View(new TracePlace
        {
            Latitude = latitude ?? 34.9501,
            Longitude = longitude ?? 127.4872,
            Category = "산책"
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TracePlace place)
    {
        if (!ModelState.IsValid)
        {
            return View(place);
        }

        if (string.IsNullOrWhiteSpace(CurrentUserId())) return Challenge();

        ApplyWriter(place);
        var created = await _places.AddAsync(place);

        TempData["Message"] = "새 장소가 저장되었습니다. 지도와 목록에 바로 반영됩니다.";

        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var place = await _places.GetByIdAsync(id, CurrentUserId());
        if (place is null) return NotFound();
        if (!place.CanModify) return Forbid();

        return View(place);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TracePlace place)
    {
        if (id != place.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(place);
        }

        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var result = await _places.UpdateAsync(place, userId);
        if (result == PlaceWriteResult.NotFound) return NotFound();
        if (result == PlaceWriteResult.Forbidden) return Forbid();

        TempData["Message"] = "장소 정보가 수정되었습니다.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var result = await _places.DeleteAsync(id, userId);
        if (result == PlaceWriteResult.NotFound) return NotFound();
        if (result == PlaceWriteResult.Forbidden) return Forbid();

        TempData["Message"] = "장소가 삭제되었습니다. 목록과 지도에서 함께 제거됩니다.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkVisited(int id)
    {
        var result = await MutateVisitAsync(id, userId => _places.MarkVisitedAsync(id, userId));
        if (result is not null) return result;

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVisit(int id)
    {
        var result = await MutateVisitAsync(id, userId => _places.AddVisitAsync(id, userId));
        if (result is not null) return result;

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveVisit(int id)
    {
        var result = await MutateVisitAsync(id, userId => _places.RemoveVisitAsync(id, userId));
        if (result is not null) return result;

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IActionResult?> MutateVisitAsync(int id, Func<string, Task<PlaceWriteResult>> mutation)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var result = await mutation(userId);
        if (result == PlaceWriteResult.NotFound) return NotFound();
        if (result == PlaceWriteResult.Forbidden) return Forbid();

        return null;
    }

    private void ApplyWriter(TracePlace place)
    {
        var userId = CurrentUserId();
        place.UserId = userId;
        place.UserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email);
        place.IsAnonymous = string.IsNullOrWhiteSpace(userId);
    }

    private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IEnumerable<string> SplitCategories(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return [];
        }

        return category
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(categoryName => !string.IsNullOrWhiteSpace(categoryName));
    }

    private static bool HasCategory(string? placeCategory, string selectedCategory)
    {
        return SplitCategories(placeCategory)
            .Any(categoryName => string.Equals(
                categoryName,
                selectedCategory,
                StringComparison.OrdinalIgnoreCase));
    }
}
