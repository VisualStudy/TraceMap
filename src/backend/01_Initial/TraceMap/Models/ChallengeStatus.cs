namespace TraceMap.Models;

public class ChallengeStatus
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsCompleted { get; set; }
    public int Current { get; set; }
    public int Target { get; set; }
}
