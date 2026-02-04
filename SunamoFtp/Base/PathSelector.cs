namespace SunamoFtp.Base;

/// <summary>
/// Manages path navigation and tokenization for FTP operations
/// </summary>
public class PathSelector
{
    private readonly bool firstTokenMustExists;

    /// <summary>
    /// Index of the first valid token (0 for relative paths, 1 for absolute paths)
    /// </summary>
    public int IndexZero;

    /// <summary>
    /// List of path Tokens split by delimiter
    /// </summary>
    public List<string> Tokens = new();

    /// <summary>
    /// Initializes path selector with initial directory. Works with both \ and / delimiters.
    /// </summary>
    /// <param name="initialDirectory">Initial directory path (e.g., C:\, www, or any root folder)</param>
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
                    var secondSlashIndex = initialDirectory.IndexOf('/', 1);
                    FirstToken = initialDirectory.Substring(0, secondSlashIndex);
                }

                var firstSlashIndex = initialDirectory.IndexOf('/');
                FirstToken = initialDirectory.Substring(0, firstSlashIndex);
            }
        }

        if (firstTokenMustExists) IndexZero = 1;
        ActualPath = initialDirectory;
    }

    /// <summary>
    /// Path delimiter character (\ for Windows paths, / for FTP paths)
    /// </summary>
    public string Delimiter { get; } = "";

    /// <summary>
    /// First token in the path (e.g., drive letter for Windows, root folder for FTP)
    /// </summary>
    public string FirstToken { get; } = "";

    private int Count => Tokens.Count;

    /// <summary>
    /// Indicates whether it's possible to navigate to parent folder
    /// </summary>
    public bool CanGoToUpFolder => Count > IndexZero;

    /// <summary>
    /// Gets or sets the current path as a delimited string
    /// </summary>
    public string
        ActualPath
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
    public List<string> DivideToTokens(string path)
    {
        return path.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// Removes the last token from path without validation (forced removal)
    /// </summary>
    public void RemoveLastTokenForce()
    {
        Tokens.RemoveAt(Count - 1);
    }

    /// <summary>
    /// Removes the last token from path with validation (throws if at root level)
    /// </summary>
    /// <exception cref="Exception">Thrown when attempting to go above root folder</exception>
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
    public string GetLastToken()
    {
        return Tokens[Count - 1];
    }

    /// <summary>
    /// Adds a new token to the end of the current path
    /// </summary>
    /// <param name="token">Token to add</param>
    public void AddToken(string token)
    {
        Tokens.Add(token);
    }
}