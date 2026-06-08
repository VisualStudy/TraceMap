using Microsoft.AspNetCore.Mvc;
using TraceMap.Services;

namespace TraceMap.Controllers.Api;

[ApiController]
[Route("api/challenges")]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challenges;

    public ChallengesController(IChallengeService challenges)
    {
        _challenges = challenges;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _challenges.GetStatusesAsync());
}
