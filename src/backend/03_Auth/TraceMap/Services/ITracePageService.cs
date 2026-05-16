using TraceMap.Models;

namespace TraceMap.Services;

public interface ITracePageService
{
    IReadOnlyList<TracePage> GetPages();
    TracePage? GetPage(string key);
}
