namespace SunamoFtp._sunamo.SunamoUri;

internal class UH
{
    internal static string GetFileName(string rp, bool wholeUrl = false)
    {
        if (wholeUrl)
        {
            var data = SHParts.RemoveAfterFirst(rp, "?");
            //var result = FS.ReplaceInvalidFileNameChars(data, []Chars);
            return data;
        }

        rp = SHParts.RemoveAfterFirst(rp, "?");
        rp = rp.TrimEnd('/');
        var dex = rp.LastIndexOf('/');
        return rp.Substring(dex + 1);
    }

    internal static string Combine(bool dir, params string[] p)
    {
        var vr = string.Join('/', p).Replace("///", "/").Replace("//", "/")
            .TrimEnd('/').Replace(":/", "://");
        if (dir) vr += "/";
        return vr;
    }
}