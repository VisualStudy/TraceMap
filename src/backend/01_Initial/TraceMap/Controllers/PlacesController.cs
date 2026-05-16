using Microsoft.AspNetCore.Mvc;
using TraceMap.Models;
using TraceMap.Services;

namespace TraceMap.Controllers;

public class PlacesController : Controller
{
    private readonly ITracePlaceService _places;

    public PlacesController(ITracePlaceService places)
    {
        _places = places;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _places.GetAllAsync());
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
        if (!ModelState.IsValid) return View(place);
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
        if (id != place.Id) return BadRequest();
        if (!ModelState.IsValid) return View(place);
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
}
