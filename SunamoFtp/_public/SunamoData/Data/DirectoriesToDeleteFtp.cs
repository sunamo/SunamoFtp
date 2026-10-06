namespace SunamoFtp._public.SunamoData.Data;

public class DirectoriesToDeleteFtp
{
    public List<Dictionary<string, List<string>>> Directories { get; set; } = new();

    public int Depth { get; set; } = 0;
}
