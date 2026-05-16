using Microsoft.AspNetCore.Identity;

namespace TraceMap.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "TraceMap 사용자";
}
