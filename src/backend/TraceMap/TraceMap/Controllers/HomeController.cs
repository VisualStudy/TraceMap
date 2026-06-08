using Microsoft.AspNetCore.Mvc;
using TraceMap.Services;

namespace TraceMap.Controllers;

public class HomeController : Controller
{
    private readonly ITracePageService _pages;
    private readonly ITracePlaceService _places;
    private readonly IChallengeService _challenges;

    public HomeController(ITracePageService pages, ITracePlaceService places, IChallengeService challenges)
    {
        _pages = pages;
        _places = places;
        _challenges = challenges;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Pages = _pages.GetPages();
        ViewBag.Places = await _places.GetAllAsync();
        ViewBag.Challenges = await _challenges.GetStatusesAsync();
        return View();
    }
}
