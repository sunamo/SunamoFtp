namespace SunamoFtp._sunamo.SunamoStringJoin;

internal class SHJoin
{
    internal static string JoinFromIndex(int startIndex, object delimiter, IList parts)
    {
        var delimiterString = delimiter.ToString();
        var stringBuilder = new StringBuilder();
        var currentIndex = 0;
        foreach (var item in parts)
        {
            if (currentIndex >= startIndex) stringBuilder.Append(item + delimiterString);
            currentIndex++;
        }

        var result = stringBuilder.ToString();
        return result.Substring(0, result.Length - 1);
    }
}
