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
        var allPlaces = await _places.GetAllAsync();

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
        var place = await _places.GetByIdAsync(id);

        return place is null ? NotFound() : View(place);
    }

    public IActionResult Create(double? latitude, double? longitude)
    {
        return View(new TracePlace
        {
            Latitude = latitude ?? 34.9501,
            Longitude = longitude ?? 127.4872,
            Category = "산책"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TracePlace place)
    {
        if (!ModelState.IsValid)
        {
            return View(place);
        }

        ApplyWriter(place);
        var created = await _places.AddAsync(place);

        TempData["Message"] = "새 장소가 저장되었습니다. 지도와 목록에 바로 반영됩니다.";

        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var place = await _places.GetByIdAsync(id);

        return place is null ? NotFound() : View(place);
    }

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

        ApplyWriter(place);
        await _places.UpdateAsync(place);

        TempData["Message"] = "장소 정보가 수정되었습니다.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _places.DeleteAsync(id);

        TempData["Message"] = "장소가 삭제되었습니다. 목록과 지도에서 함께 제거됩니다.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkVisited(int id)
    {
        await _places.MarkVisitedAsync(id);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVisit(int id)
    {
        await _places.AddVisitAsync(id);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveVisit(int id)
    {
        await _places.RemoveVisitAsync(id);

        return RedirectToAction(nameof(Details), new { id });
    }


    private void ApplyWriter(TracePlace place)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        place.UserId = userId;
        place.UserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email);
        place.IsAnonymous = string.IsNullOrWhiteSpace(userId);
    }

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