namespace SunamoFtp.FtpClients;

public partial class FtpNet : FtpBase
{
    /// <summary>
    ///     OK
    ///     LIST
    ///     This is entry method, do not call GetFSEntriesListRecursively with 5 parameters even though it has same name
    ///     Returns files and folders, but note that folders are always after files
    /// </summary>
    /// <param name = "foldersToSkip"></param>
    public override Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip)
    {
        // Must store path to entire folder, not just current folder name
        var visitedFolders = new List<string>();
        var result = new Dictionary<string, List<string>>();
        var ftpEntries = ListDirectoryDetails();
        var actualPath = PathSelector.ActualPath;
        OnNewStatus("Getting recursive file list from folder" + " " + actualPath);
        foreach (var item in ftpEntries)
        {
            var firstChar = item[0];
            if (firstChar == '-')
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
            else if (firstChar == 'd')
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
                //GetFSEntriesListRecursively(foldersToSkip, visitedFolders, result, PathSelector.ActualPath, folderName);
                }
            }
            else
            {
                throw new Exception("Unsupported object type");
            }
        }

        return result;
    }

    /// <summary>
    ///     OK
    ///     Tuto metodu nepoužívej, protože fakticky způsobuje neošetřenou výjimku, pokud již cesta bude skutečně / a a nebude
    ///     moci se přesunout nikde výš
    /// </summary>
    public override void GoToUpFolderForce()
    {
        if (FtpLogging.GoToUpFolder)
            OnNewStatus("Navigating to parent folder" + " " + PathSelector.ActualPath);
        PathSelector.RemoveLastTokenForce();
        OnNewStatusNewFolder();
    }

    /// <summary>
    ///     OK
    /// </summary>
    public override void GoToUpFolder()
    {
        if (PathSelector.CanGoToUpFolder)
        {
            PathSelector.RemoveLastToken();
            OnNewStatusNewFolder();
        }
        else
        {
            OnNewStatus("Could not navigate to parent folder" + ".");
        }
    }

    /// <summary>
    /// isDebug output for current folder path (not implemented)
    /// </summary>
    public override void DebugActualFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// isDebug output method for logging FTP operations (not implemented)
    /// </summary>
    /// <param name="context">Operation or context identifier</param>
    /// <param name="text">Message format string</param>
    /// <param name="args">Format arguments</param>
    public override void WriteDebugLog(string context, string text, params object[] args)
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