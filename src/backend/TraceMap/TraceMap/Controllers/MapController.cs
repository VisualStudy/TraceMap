using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    public async Task<IActionResult> Index(int? selectedId)
    {
        ViewBag.SelectedId = selectedId;
        return View(await _places.GetAllAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
    }

    [Authorize]
    [HttpGet]
    public IActionResult SelectLocation(double? latitude, double? longitude)
    {
        ViewBag.HasExplicitLocation = latitude.HasValue && longitude.HasValue;
        ViewBag.Latitude = latitude ?? 34.9501;
        ViewBag.Longitude = longitude ?? 127.4872;
        return View();
    }
}
