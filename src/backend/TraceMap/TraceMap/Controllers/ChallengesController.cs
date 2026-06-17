using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TraceMap.Services;

namespace TraceMap.Controllers;

[Authorize]
public class ChallengesController : Controller
{
    private readonly IChallengeService _challenges;

    public ChallengesController(IChallengeService challenges)
    {
        _challenges = challenges;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _challenges.GetStatusesAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
    }
}
