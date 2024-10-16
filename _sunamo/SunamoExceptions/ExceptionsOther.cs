namespace SunamoFtp._sunamo.SunamoExceptions;
partial class Exceptions
{
    private static string CheckBefore(string before)
    {
        return string.IsNullOrWhiteSpace(before) ? "" : before + ": ";
    }
    internal static string TextOfExceptions(Exception ex, bool alsoInner = true)
    {
        if (ex == null) return "";
        StringBuilder sb = new();
        sb.Append("Exception:");
        sb.AppendLine(ex.Message);
        if (alsoInner)
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.AppendLine(ex.Message);
            }
        var r = sb.ToString();
        return r;
    }
}