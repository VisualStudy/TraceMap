using TraceMap.Models;

namespace TraceMap.ViewModels;

public class PlacesIndexViewModel
{
    public IReadOnlyList<TracePlace> Places { get; set; } = [];

    public IReadOnlyList<string> Categories { get; set; } = [];

    public string? SelectedCategory { get; set; }

    public int TotalCount { get; set; }

    public int FilteredCount => Places.Count;

    public bool IsAllSelected => string.IsNullOrWhiteSpace(SelectedCategory);
}
