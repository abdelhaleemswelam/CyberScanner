namespace CyberScanner.Models;

public class ScanProfile
{
    public string Name { get; set; }
    public string StartIP { get; set; }
    public string EndIP { get; set; }
    public int TimeoutMs { get; set; } = 1000;
    public int MaxThreads { get; set; } = 50;
    public DateTime Created { get; set; } = DateTime.Now;
}