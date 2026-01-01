namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public partial class FtpNet : FtpBase
{
    /// <summary>
    ///     OK
    ///     LIST
    ///     Toto je vstupní metoda, metodu getFSEntriesListRecursively s 5ti parametry nevolej, ač má stejný název
    ///     Vrátí files i složky, ale pozor, složky jsou vždycky až po souborech
    /// </summary>
    /// <param name = "foldersToSkip"></param>
    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> foldersToSkip)
    {
        // Musí se do ní ukládat cesta k celé složce, nikoliv jen název aktuální složky
        var visitedFolders = new List<string>();
        var result = new Dictionary<string, List<string>>();
        var ftpEntries = ListDirectoryDetails();
        var actualPath = PathSelector.ActualPath;
        OnNewStatus("Získávám rekurzivní seznam souborů ze složky" + " " + actualPath);
        foreach (var item in ftpEntries)
        {
            var fz = item[0];
            if (fz == '-')
            {
                if (result.ContainsKey(actualPath))
                {
                    result[actualPath].Add(item);
                }
                else
                {
                    var entries = new List<string>();
                    entries.Add(item);
                    result.Add(actualPath, entries);
                }
            }
            else if (fz == 'd')
            {
                var folderName = SHJoin.JoinFromIndex(8, ' ', SHSplit.Split(item, ""));
                if (!FtpHelper.IsThisOrUp(folderName))
                {
                    if (result.ContainsKey(actualPath))
                    {
                        result[actualPath].Add(item + "/");
                    }
                    else
                    {
                        var entries = new List<string>();
                        entries.Add(item + "/");
                        result.Add(actualPath, entries);
                    }
                //getFSEntriesListRecursively(foldersToSkip, visitedFolders, result, PathSelector.ActualPath, folderName);
                }
            }
            else
            {
                throw new Exception("Nepodporovaný typ objektu");
            }
        }

        return result;
    }

    /// <summary>
    ///     OK
    ///     Tuto metodu nepoužívej, protože fakticky způsobuje neošetřenou výjimku, pokud již cesta bude skutečně / a a nebude
    ///     moci se přesunout nikde výš
    /// </summary>
    public override void goToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder)
            OnNewStatus("Přecházím do nadsložky" + " " + PathSelector.ActualPath);
        PathSelector.RemoveLastTokenForce();
        OnNewStatusNewFolder();
    }

    /// <summary>
    ///     OK
    /// </summary>
    public override void goToUpFolder()
    {
        if (PathSelector.CanGoToUpFolder)
        {
            PathSelector.RemoveLastToken();
            OnNewStatusNewFolder();
        }
        else
        {
            OnNewStatus("Nemohl jsem přejít do nadsložky" + ".");
        }
    }

    /// <summary>
    /// Debug output for current folder path (not implemented)
    /// </summary>
    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Debug output method for logging FTP operations (not implemented)
    /// </summary>
    /// <param name="what">Operation or context identifier</param>
    /// <param name="text">Message format string</param>
    /// <param name="args">Format arguments</param>
    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Establishes connection to the FTP server (not implemented)
    /// </summary>
    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}