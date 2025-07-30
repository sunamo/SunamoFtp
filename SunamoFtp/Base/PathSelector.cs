namespace SunamoFtp.Base;

public class PathSelector
{
    private static Type type = typeof(PathSelector);
    private readonly bool firstTokenMustExists;
    public int indexZero;
    public List<string> tokens = new();

    /// <summary>
    ///     A1 je složka, která je nejvyšší. Může být nastavena na C:\, www, SE nebo cokoliv jiného
    ///     Pracuje buď s \ nebo s / - podle toho co najde v A1. Libovolně lze přidat další oddělovače
    /// </summary>
    /// <param name="initialDirectory"></param>
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
                    throw new Exception("Počáteční složka nemůže začínat s lomítkem na začátku");
                    var druhy = initialDirectory.IndexOf('/', 1);
                    FirstToken = initialDirectory.Substring(0, druhy);
                }

                var prvni = initialDirectory.IndexOf('/');
                FirstToken = initialDirectory.Substring(0, prvni);
            }
        }

        if (firstTokenMustExists) indexZero = 1;
        ActualPath = initialDirectory;
    }

    public string Delimiter { get; } = "";

    public string FirstToken { get; } = "";

    private int Count => tokens.Count;

    public bool CanGoToUpFolder => Count > indexZero;

    public string
        ActualPath
    {
        get
        {
            if (tokens.Count != 0)
                return string.Join(Delimiter, tokens.ToArray()) + Delimiter;
            return "/";
        }
        set
        {
            tokens.Clear();
            tokens.AddRange(value.Split(new[] { Delimiter },
                StringSplitOptions.RemoveEmptyEntries)); //SHSplit.Split(value, delimiter));
        }
    }

    public List<string> DivideToTokens(string r)
    {
        return r.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void RemoveLastTokenForce()
    {
        tokens.RemoveAt(Count - 1);
    }

    public void RemoveLastToken()
    {
        if (CanGoToUpFolder)
            tokens.RemoveAt(Count - 1);
        else
            throw new Exception("Is not possible go to up folder");
    }

    public string GetLastToken()
    {
        return tokens[Count - 1];
    }

    public void AddToken(string token)
    {
        tokens.Add(token);
    }
}