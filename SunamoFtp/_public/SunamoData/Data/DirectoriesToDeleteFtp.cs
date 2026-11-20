namespace SunamoFtp._public.SunamoData.Data;

/// <summary>
/// Represents the directories to delete ftp type.
/// </summary>
public class DirectoriesToDeleteFtp
{
    /// <summary>
    /// Gets or sets the directories.
    /// </summary>
    public List<Dictionary<string, List<string>>> Directories { get; set; } = new();

    /// <summary>
    /// Gets or sets the depth.
    /// </summary>
    public int Depth { get; set; } = 0;
}
