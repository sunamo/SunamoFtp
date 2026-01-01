namespace SunamoFtp._sunamo.SunamoExceptions;

/// <summary>
/// Exception handling and stack trace utilities
/// </summary>
// © www.sunamo.cz. All Rights Reserved.
internal sealed partial class Exceptions
{
    #region Other
    /// <summary>
    /// Prepares prefix text for exception messages
    /// </summary>
    /// <param name="before">Prefix text to check and format</param>
    /// <returns>Formatted prefix with colon and space, or empty string</returns>
    internal static string CheckBefore(string before)
    {
        return string.IsNullOrWhiteSpace(before) ? string.Empty : before + ": ";
    }

    /// <summary>
    /// Extracts place of exception from stack trace
    /// </summary>
    /// <param name="isFillAlsoFirstTwo">If true, fills type and method name from first non-ThrowEx frame</param>
    /// <returns>Tuple containing type name, method name, and full stack trace</returns>
    internal static Tuple<string, string, string> PlaceOfException(bool isFillAlsoFirstTwo = true)
    {
        StackTrace stackTrace = new();
        var value = stackTrace.ToString();
        var lines = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        lines.RemoveAt(0);
        var currentLineIndex = 0;
        string type = string.Empty;
        string methodName = string.Empty;
        for (; currentLineIndex < lines.Count; currentLineIndex++)
        {
            var item = lines[currentLineIndex];
            if (isFillAlsoFirstTwo)
                if (!item.StartsWith("   at ThrowEx"))
                {
                    TypeAndMethodName(item, out type, out methodName);
                    isFillAlsoFirstTwo = false;
                }
            if (item.StartsWith("at System."))
            {
                lines.Add(string.Empty);
                lines.Add(string.Empty);
                break;
            }
        }
        return new Tuple<string, string, string>(type, methodName, string.Join(Environment.NewLine, lines));
    }

    /// <summary>
    /// Extracts type and method name from stack trace line
    /// </summary>
    /// <param name="stackTraceLine">Single line from stack trace</param>
    /// <param name="type">Extracted type name</param>
    /// <param name="methodName">Extracted method name</param>
    internal static void TypeAndMethodName(string stackTraceLine, out string type, out string methodName)
    {
        var trimmedLine = stackTraceLine.Split("at ")[1].Trim();
        var text = trimmedLine.Split("(")[0];
        var segments = text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        methodName = segments[^1];
        segments.RemoveAt(segments.Count - 1);
        type = string.Join(".", segments);
    }

    /// <summary>
    /// Gets calling method name from stack trace
    /// </summary>
    /// <param name="frameDepth">Depth in stack trace (default 1)</param>
    /// <returns>Method name of calling method</returns>
    internal static string CallingMethod(int frameDepth = 1)
    {
        StackTrace stackTrace = new();
        var methodBase = stackTrace.GetFrame(frameDepth)?.GetMethod();
        if (methodBase == null)
        {
            return "Method name cannot be get";
        }
        var methodName = methodBase.Name;
        return methodName;
    }
    #endregion

    #region IsNullOrWhitespace
    internal readonly static StringBuilder AdditionalInfoInnerStringBuilder = new();
    internal readonly static StringBuilder AdditionalInfoStringBuilder = new();
    #endregion

    #region OnlyReturnString
    /// <summary>
    /// Returns "Not supported" exception message
    /// </summary>
    /// <param name="before">Prefix text</param>
    /// <returns>Formatted exception message</returns>
    internal static string? NotSupported(string before)
    {
        return CheckBefore(before) + "Not supported";
    }

    /// <summary>
    /// Returns "Not implemented method" exception message
    /// </summary>
    /// <param name="before">Prefix text</param>
    /// <returns>Formatted exception message</returns>
    internal static string? NotImplementedMethod(string before)
    {
        return CheckBefore(before) + "Not implemented method.";
    }
    #endregion
}
