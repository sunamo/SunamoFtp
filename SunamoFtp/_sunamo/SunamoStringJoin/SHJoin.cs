namespace SunamoFtp._sunamo.SunamoStringJoin;

/// <summary>
/// Helper class for string joining operations
/// </summary>
internal class SHJoin
{
    /// <summary>
    /// Joins list elements starting from specified index with delimiter
    /// </summary>
    /// <param name="startIndex">Index to start joining from</param>
    /// <param name="delimiter">Delimiter to use between elements</param>
    /// <param name="parts">List of parts to join</param>
    /// <returns>Joined string</returns>
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