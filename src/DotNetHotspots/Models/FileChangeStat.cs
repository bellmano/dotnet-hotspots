namespace DotNetHotspots.Models;

public class FileChangeStat
{
    public string FilePath { get; set; } = string.Empty;
    public int ChangeCount { get; set; }
}
