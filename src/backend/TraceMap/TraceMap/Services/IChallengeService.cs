using TraceMap.Models;

namespace TraceMap.Services;

public interface IChallengeService
{
    Task<List<ChallengeStatus>> GetStatusesAsync(string? userId);
}
