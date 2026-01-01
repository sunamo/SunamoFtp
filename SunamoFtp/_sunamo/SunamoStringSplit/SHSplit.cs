namespace SunamoFtp._sunamo.SunamoStringSplit;

/// <summary>
/// Helper class for string splitting operations
/// </summary>
internal class SHSplit
{
    /// <summary>
    /// Splits a string by specified delimiters and removes empty entries
    /// </summary>
    /// <param name="text">Text to split</param>
    /// <param name="delimiters">Delimiters to split by</param>
    /// <returns>List of non-empty string parts</returns>
    internal static List<string> Split(string text, params string[] delimiters)
    {
        return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// Splits a string by specified character delimiters
    /// </summary>
    /// <param name="text">Text to split</param>
    /// <param name="delimiters">Character delimiters to split by</param>
    /// <returns>List of string parts</returns>
    internal static List<string> SplitChar(string text, params char[] delimiters)
    {
        return text.Split(delimiters).ToList();
    }
}
