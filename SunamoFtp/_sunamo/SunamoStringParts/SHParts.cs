namespace SunamoFtp._sunamo.SunamoStringParts;

/// <summary>
/// Helper class for string manipulation operations
/// </summary>
internal class SHParts
{
    /// <summary>
    /// Removes everything after the first occurrence of specified delimiter
    /// </summary>
    /// <param name="text">Text to process</param>
    /// <param name="delimiter">Delimiter to search for</param>
    /// <returns>Text up to first delimiter, or original text if delimiter not found</returns>
    internal static string RemoveAfterFirst(string text, string delimiter)
    {
        var index = text.IndexOf(delimiter);
        if (index == -1 || index == text.Length - 1) return text;

        var result = text.Remove(index);
        return result;
    }
}
