using Microsoft.AspNetCore.Mvc;
using TraceMap.Services;

namespace TraceMap.Controllers.Api;

[ApiController]
[Route("api/pages")]
public class PagesController : ControllerBase
{
    private readonly ITracePageService _pages;

    public PagesController(ITracePageService pages)
    {
        _pages = pages;
    }

    [HttpGet]
    public IActionResult Get() => Ok(_pages.GetPages());

    [HttpGet("{key}")]
    public IActionResult Get(string key)
    {
        var page = _pages.GetPage(key);
        return page is null ? NotFound() : Ok(page);
    }
}
