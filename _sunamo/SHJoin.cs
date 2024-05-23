using System.Collections;

namespace SunamoFtp;
public class SHJoin
{
    public static string JoinFromIndex(int dex, object delimiter2, IList parts)
    {
        string delimiter = delimiter2.ToString();
        StringBuilder sb = new StringBuilder();
        int i = 0;
        foreach (var item in parts)
        {
            if (i >= dex)
            {
                sb.Append(item + delimiter);
            }
            i++;
        }
        string vr = sb.ToString();
        return vr.Substring(0, vr.Length - 1);
        //return SHSubstring.SubstringLength(vr, 0, vr.Length - 1);
    }
}
