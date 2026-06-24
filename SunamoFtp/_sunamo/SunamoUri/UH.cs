namespace SunamoFtp._sunamo.SunamoUri;

internal class UH
{
    internal static string GetFileName(string path, bool isWholeUrl = false)
    {
        if (isWholeUrl)
        {
            var data = SHParts.RemoveAfterFirst(path, "?");
            return data;
        }

        path = SHParts.RemoveAfterFirst(path, "?");
        path = path.TrimEnd('/');
        var lastSlashIndex = path.LastIndexOf('/');
        return path.Substring(lastSlashIndex + 1);
    }

    internal static string Combine(bool isDirectory, params string[] parts)
    {
        var result = string.Join("/", parts).Replace("///", "/").Replace("//", "/")
            .TrimEnd('/').Replace(":/", "://");
        if (isDirectory) result += "/";
        return result;
    }
}
