namespace TraceMap.Models;

public class TracePage
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Path { get; set; } = "";
    public string Description { get; set; } = "";
    public bool RequiresAuth { get; set; }
    public string Icon { get; set; } = "";
    public int DisplayOrder { get; set; }
}
