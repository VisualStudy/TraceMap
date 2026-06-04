using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TraceMap.Controllers.Api;

[ApiController]
[Route("api/secure")]
public class SecureController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "인증된 사용자만 접근할 수 있는 TraceMap 보호 API입니다.",
            userName = User.Identity?.Name
        });
    }
}
