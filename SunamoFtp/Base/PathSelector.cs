namespace SunamoFtp.Base;

public class PathSelector
{
    private readonly bool firstTokenMustExists;

    public int IndexZero;

    public List<string> Tokens = new();

    // Works with both \ and / delimiters.
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

    public string Delimiter { get; } = "";

    public string FirstToken { get; } = "";

    private int Count => Tokens.Count;

    public bool CanGoToUpFolder => Count > IndexZero;

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

    public void RemoveLastTokenForce()
    {
        Tokens.RemoveAt(Count - 1);
    }

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

    public void AddToken(string token)
    {
        Tokens.Add(token);
    }
}
