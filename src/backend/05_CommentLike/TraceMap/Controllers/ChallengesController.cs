using Microsoft.AspNetCore.Mvc;
using TraceMap.Services;

namespace TraceMap.Controllers;

public class ChallengesController : Controller
{
    private readonly IChallengeService _challenges;

    public ChallengesController(IChallengeService challenges)
    {
        _challenges = challenges;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _challenges.GetStatusesAsync());
    }
}
