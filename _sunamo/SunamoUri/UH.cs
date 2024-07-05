

namespace SunamoFtp._sunamo.SunamoUri;
internal class UH
{
    internal static string GetFileName(string rp, bool wholeUrl = false)
    {
        if (wholeUrl)
        {
            var d = SHParts.RemoveAfterFirst(rp, AllStrings.q);
            //var result = FS.ReplaceInvalidFileNameChars(d, EmptyArrays.Chars);
            return d;
        }
        rp = SHParts.RemoveAfterFirst(rp, AllStrings.q);
        rp = rp.TrimEnd(AllChars.slash);
        int dex = rp.LastIndexOf(AllChars.slash);
        return rp.Substring(dex + 1);
    }

    internal static string Combine(bool dir, params string[] p)
    {
        string vr = string.Join(AllChars.slash, p).Replace("///", AllStrings.slash).Replace("//", AllStrings.slash).TrimEnd(AllChars.slash).Replace(":/", "://");
        if (dir)
        {
            vr += AllStrings.slash;
        }
        return vr;
    }
}
