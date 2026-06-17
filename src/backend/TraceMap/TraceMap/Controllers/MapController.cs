using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TraceMap.Services;

namespace TraceMap.Controllers;

public class MapController : Controller
{
    private readonly ITracePlaceService _places;

    public MapController(ITracePlaceService places)
    {
        _places = places;
    }

    public async Task<IActionResult> Index(int? selectedId)
    {
        ViewBag.SelectedId = selectedId;
        return View(await _places.GetAllAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
    }

    [HttpGet]
    public IActionResult SelectLocation(double? latitude, double? longitude)
    {
        ViewBag.Latitude = latitude ?? 34.9501;
        ViewBag.Longitude = longitude ?? 127.4872;
        return View();
    }
}
