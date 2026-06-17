using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        ViewBag.Pages = _pages.GetPages();
        ViewBag.Places = await _places.GetAllAsync(userId);
        ViewBag.Challenges = string.IsNullOrWhiteSpace(userId)
            ? new List<TraceMap.Models.ChallengeStatus>()
            : await _challenges.GetStatusesAsync(userId);

        return View();
    }
}
