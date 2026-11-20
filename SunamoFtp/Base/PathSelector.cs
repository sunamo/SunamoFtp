namespace SunamoFtp.Base;

/// <summary>
/// Represents the path selector type.
/// </summary>
public class PathSelector
{
    private readonly bool firstTokenMustExists;

    /// <summary>
    /// The index zero.
    /// </summary>
    public int IndexZero;

    /// <summary>
    /// The tokens.
    /// </summary>
    public List<string> Tokens = new();

    // Works with both \ and / delimiters.
    /// <summary>
    /// Path selector.
    /// </summary>
    /// <returns>The path selector result.</returns>
    public PathSelector(string initialDirectory)
    {
        if (initialDirectory.Contains(":\\") || initialDirectory != "") firstTokenMustExists = true;
        if (initialDirectory.Contains("\""))
        {
            Delimiter = "\"";
        }
        else
        {
            Delimiter = "/";
            if (initialDirectory.Contains(Delimiter))
            {
                if (initialDirectory.StartsWith("/"))
                {
                    throw new Exception("Initial directory cannot start with a leading slash");
                }

                var firstSlashIndex = initialDirectory.IndexOf('/');
                FirstToken = initialDirectory.Substring(0, firstSlashIndex);
            }
        }

        if (firstTokenMustExists) IndexZero = 1;
        ActualPath = initialDirectory;
    }

    /// <summary>
    /// Gets the delimiter.
    /// </summary>
    public string Delimiter { get; } = "";

    /// <summary>
    /// Gets the first token.
    /// </summary>
    public string FirstToken { get; } = "";

    private int Count => Tokens.Count;

    /// <summary>
    /// Gets the can go to up folder.
    /// </summary>
    public bool CanGoToUpFolder => Count > IndexZero;

    /// <summary>
    /// The actual path.
    /// </summary>
    public string ActualPath
    {
        get
        {
            if (Tokens.Count != 0)
                return string.Join(Delimiter, Tokens.ToArray()) + Delimiter;
            return "/";
        }
        set
        {
            Tokens.Clear();
            Tokens.AddRange(value.Split(new[] { Delimiter },
                StringSplitOptions.RemoveEmptyEntries)); //SHSplit.Split(value, delimiter));
        }
    }

    /// <summary>
    /// Splits a path string into individual tokens using the configured delimiter
    /// </summary>
    /// <param name="path">Path string to divide</param>
    /// <returns>List of path tokens</returns>
    public List<string> DivideToTokens(string path) =>
        path.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();

    /// <summary>
    /// Remove last token force.
    /// </summary>
    /// <returns>The remove last token force result.</returns>
    public void RemoveLastTokenForce()
    {
        Tokens.RemoveAt(Count - 1);
    }

    /// <summary>
    /// Initializes a new instance of the remove last token class.
    /// </summary>
    /// <param name="StringSplitOptions.RemoveEmptyEntries)">The string split options remove empty entries.</param>
    public void RemoveLastToken()
    {
        if (CanGoToUpFolder)
            Tokens.RemoveAt(Count - 1);
        else
            throw new Exception("Is not possible go to up folder");
    }

    /// <summary>
    /// Gets the last token in the current path
    /// </summary>
    /// <returns>Last path token</returns>
    public string GetLastToken() => Tokens[Count - 1];

    /// <summary>
    /// Add token.
    /// </summary>
    /// <param name="1">The 1.</param>
    /// <returns>The add token result.</returns>
    public void AddToken(string token)
    {
        Tokens.Add(token);
    }
}
