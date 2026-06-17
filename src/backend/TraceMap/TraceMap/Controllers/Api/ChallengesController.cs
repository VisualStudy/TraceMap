using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TraceMap.Services;

namespace TraceMap.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/challenges")]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challenges;

    public ChallengesController(IChallengeService challenges)
    {
        _challenges = challenges;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _challenges.GetStatusesAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
}
