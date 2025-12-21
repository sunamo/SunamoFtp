namespace SunamoFtp._sunamo.SunamoStringJoin;

internal class SHJoin
{
    internal static string JoinFromIndex(int dex, object delimiter2, IList parts)
    {
        var delimiter = delimiter2.ToString();
        var stringBuilder = new StringBuilder();
        var i = 0;
        foreach (var item in parts)
        {
            if (i >= dex) stringBuilder.Append(item + delimiter);
            i++;
        }

        var result = stringBuilder.ToString();
        return result.Substring(0, result.Length - 1);
        //return SHSubstring.SubstringLength(result, 0, result.Length - 1);
    }
}