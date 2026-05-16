using Microsoft.AspNetCore.Mvc;
using TraceMap.Services;

namespace TraceMap.Controllers;

public class RecommendationsController : Controller
{
    private readonly ITracePlaceService _places;

    public RecommendationsController(ITracePlaceService places)
    {
        _places = places;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _places.GetSharedAsync());
    }
}
