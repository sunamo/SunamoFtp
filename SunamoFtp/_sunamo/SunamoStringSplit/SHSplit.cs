namespace SunamoFtp._sunamo.SunamoStringSplit;

internal class SHSplit
{
    internal static List<string> Split(string text, params string[] delimiters)
    {
        return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    internal static List<string> SplitChar(string text, params char[] delimiters)
    {
        return text.Split(delimiters).ToList();
    }
}
