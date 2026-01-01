namespace SunamoFtp._sunamo.SunamoExceptions;

/// <summary>
/// Exception throwing utilities with automatic stack trace information
/// </summary>
internal partial class ThrowEx
{
    /// <summary>
    /// Throws "Not implemented method" exception with full code location
    /// </summary>
    /// <returns>True if exception was thrown</returns>
    internal static bool NotImplementedMethod() { return ThrowIsNotNull(Exceptions.NotImplementedMethod); }

    /// <summary>
    /// Throws "Not supported" exception with full code location
    /// </summary>
    /// <returns>True if exception was thrown</returns>
    internal static bool NotSupported() { return ThrowIsNotNull(Exceptions.NotSupported(FullNameOfExecutedCode())); }

    #region Other
    /// <summary>
    /// Gets full name of currently executed code (Type.Method)
    /// </summary>
    /// <returns>Full qualified method name</returns>
    internal static string FullNameOfExecutedCode()
    {
        Tuple<string, string, string> placeOfException = Exceptions.PlaceOfException();
        string fullName = FullNameOfExecutedCode(placeOfException.Item1, placeOfException.Item2, true);
        return fullName;
    }

    /// <summary>
    /// Gets full name of executed code from type and method information
    /// </summary>
    /// <param name="type">Type object or type name</param>
    /// <param name="methodName">Method name (null to auto-detect)</param>
    /// <param name="isFromThrowEx">If true, adjusts stack depth for ThrowEx calls</param>
    /// <returns>Full qualified method name</returns>
    static string FullNameOfExecutedCode(object type, string methodName, bool isFromThrowEx = false)
    {
        if (methodName == null)
        {
            int depth = 2;
            if (isFromThrowEx)
            {
                depth++;
            }

            methodName = Exceptions.CallingMethod(depth);
        }
        string typeFullName;
        if (type is Type typeInfo)
        {
            typeFullName = typeInfo.FullName ?? "Type cannot be get via type is Type type2";
        }
        else if (type is MethodBase method)
        {
            typeFullName = method.ReflectedType?.FullName ?? "Type cannot be get via type is MethodBase method";
            methodName = method.Name;
        }
        else if (type is string)
        {
            typeFullName = type.ToString() ?? "Type cannot be get via type is string";
        }
        else
        {
            Type typeInstance = type.GetType();
            typeFullName = typeInstance.FullName ?? "Type cannot be get via type.GetType()";
        }
        return string.Concat(typeFullName, ".", methodName);
    }

    /// <summary>
    /// Throws exception if message is not null
    /// </summary>
    /// <param name="exception">Exception message</param>
    /// <param name="isReallyThrow">If true, actually throws the exception</param>
    /// <returns>True if exception would be thrown</returns>
    internal static bool ThrowIsNotNull(string? exception, bool isReallyThrow = true)
    {
        if (exception != null)
        {
            Debugger.Break();
            if (isReallyThrow)
            {
                throw new Exception(exception);
            }
            return true;
        }
        return false;
    }

    #region For avoid FullNameOfExecutedCode

    /// <summary>
    /// Throws exception using function that generates exception message
    /// </summary>
    /// <param name="exceptionMessageFunction">Function that generates exception message from code location</param>
    /// <returns>True if exception was thrown</returns>
    internal static bool ThrowIsNotNull(Func<string, string?> exceptionMessageFunction)
    {
        string? exception = exceptionMessageFunction(FullNameOfExecutedCode());
        return ThrowIsNotNull(exception);
    }
    #endregion
    #endregion
}
