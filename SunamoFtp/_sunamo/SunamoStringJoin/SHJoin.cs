namespace SunamoFtp._sunamo.SunamoStringJoin;

internal class SHJoin
{
    internal static string JoinFromIndex(int dex, object delimiter2, IList parts)
    {
        var delimiter = delimiter2.ToString();
        var sb = new StringBuilder();
        var i = 0;
        foreach (var item in parts)
        {
            if (i >= dex) sb.Append(item + delimiter);
            i++;
        }

        var vr = sb.ToString();
        return vr.Substring(0, vr.Length - 1);
        //return SHSubstring.SubstringLength(vr, 0, vr.Length - 1);
    }
}