namespace SunamoFtp._sunamo.SunamoStringParts;

internal class SHParts
{
    internal static string RemoveAfterFirst(string text, string delimiter)
    {
        var index = text.IndexOf(delimiter);
        if (index == -1 || index == text.Length - 1) return text;

        var result = text.Remove(index);
        return result;
    }
}
