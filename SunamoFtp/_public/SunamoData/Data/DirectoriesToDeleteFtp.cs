namespace SunamoFtp._public.SunamoData.Data;

/// <summary>
/// Represents directories to be deleted on FTP server
/// </summary>
public class DirectoriesToDeleteFtp
{
    /// <summary>
    /// List of directories organized by depth level
    /// </summary>
    public List<Dictionary<string, List<string>>> Directories { get; set; } = new();

    /// <summary>
    /// Current depth level in directory hierarchy
    /// </summary>
    public int Depth { get; set; } = 0;
}