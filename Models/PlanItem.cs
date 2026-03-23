namespace Podsetnik.Models;

public class PlanItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime AlarmTime { get; set; }
    public bool IsCompleted { get; set; }
}
