
namespace SunamoFtp.Base;
using SunamoValues;


public class PathSelector
{
    string firstToken = "";
    public List<string> tokens = new List<string>();
    bool firstTokenMustExists = false;
    string delimiter = "";
    public string Delimiter
    {
        get
        {
            return delimiter;
        }
    }
    public int indexZero = 0;

    public string FirstToken
    {
        get
        {
            return firstToken;
        }
    }

    public List<string> DivideToTokens(string r)
    {
        return r.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// A1 je složka, která je nejvyšší. Může být nastavena na C:\, www, SE nebo cokoliv jiného
    /// Pracuje buď s \ nebo s / - podle toho co najde v A1. Libovolně lze přidat další oddělovače
    /// </summary>
    /// <param name="initialDirectory"></param>
    public PathSelector(string initialDirectory)
    {
        if (initialDirectory.Contains(":\\") || initialDirectory != "")
        {
            firstTokenMustExists = true;

        }
        if (initialDirectory.Contains(AllStringsSE.bs))
        {
            delimiter = AllStringsSE.bs;
        }
        else
        {
            delimiter = AllStringsSE.slash;
            if (initialDirectory.Contains(delimiter))
            {
                if (initialDirectory.StartsWith(AllStringsSE.slash))
                {
                    throw new Exception("Počáteční složka nemůže začínat s lomítkem na začátku");
                    int druhy = initialDirectory.IndexOf(AllCharsSE.slash, 1);
                    firstToken = initialDirectory.Substring(0, druhy);
                }
                else
                {
                    int prvni = initialDirectory.IndexOf(AllCharsSE.slash);
                    firstToken = initialDirectory.Substring(0, prvni);
                }
            }
        }

        if (firstTokenMustExists)
        {
            indexZero = 1;
        }
        ActualPath = initialDirectory;
    }

    int Count
    {
        get
        {
            return tokens.Count;
        }
    }
    public void RemoveLastTokenForce()
    {
        tokens.RemoveAt(Count - 1);
    }

    static Type type = typeof(PathSelector);

    public void RemoveLastToken()
    {


        if (CanGoToUpFolder)
        {
            tokens.RemoveAt(Count - 1);
        }
        else
        {
            throw new Exception("Is not possible go to up folder");
        }
    }

    public string GetLastToken()
    {
        return tokens[Count - 1];
    }

    public void AddToken(string token)
    {
        tokens.Add(token);
    }

    public bool CanGoToUpFolder
    {
        get
        {
            return Count > indexZero;
        }
    }

    public string
        ActualPath
    {
        get
        {
            if (tokens.Count != 0)
            {
                return string.Join(delimiter, tokens.ToArray()) + delimiter;
            }
            else
            {
                return AllStringsSE.slash;
            }
        }
        set
        {
            tokens.Clear();
            tokens.AddRange(value.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)); //SHSplit.Split(value, delimiter));
        }
    }


}
