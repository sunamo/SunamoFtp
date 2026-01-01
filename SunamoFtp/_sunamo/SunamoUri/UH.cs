namespace SunamoFtp._sunamo.SunamoUri;

/// <summary>
/// Helper class for URI and path operations
/// </summary>
internal class UH
{
    /// <summary>
    /// Extracts file name from URL or path
    /// </summary>
    /// <param name="path">URL or file path</param>
    /// <param name="isWholeUrl">If true, preserves query strings</param>
    /// <returns>File name extracted from path</returns>
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

    /// <summary>
    /// Combines multiple path segments into single path
    /// </summary>
    /// <param name="isDirectory">If true, ensures trailing slash</param>
    /// <param name="paths">Path segments to combine</param>
    /// <returns>Combined path</returns>
    internal static string Combine(bool isDirectory, params string[] paths)
    {
        var result = string.Join('/', paths).Replace("///", "/").Replace("//", "/")
            .TrimEnd('/').Replace(":/", "://");
        if (isDirectory) result += "/";
        return result;
    }
}
