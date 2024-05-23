namespace SunamoFtp;
public class SHSplit
{
    public static List<string> Split(string item, params string[] space)
    {
        return item.Split(space, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public static List<string> SplitChar(string v1, params char[] v2)
    {
        return v1.Split(v2).ToList();
    }
}
