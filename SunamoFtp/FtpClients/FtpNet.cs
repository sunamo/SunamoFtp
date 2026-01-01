namespace SunamoFtp.FtpClients;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy

/// <summary>
/// FTP client implementation using FtpWebRequest
/// </summary>
public partial class FtpNet : FtpBase
{
    /// <summary>
    /// Performs login to FTP server if not already logged in
    /// </summary>
    /// <param name="startup">Indicates if this is initial startup login</param>
    public override void LoginIfIsNot(bool startup)
    {
        this.startup = startup;
    // Není potřeba se přihlašovat, přihlašovácí údaje posílám při každém příkazu
    }

    /// <summary>
    /// Navigates to specified path on FTP server, creating directories as needed
    /// </summary>
    /// <param name="remoteFolder">Remote folder path to navigate to</param>
    public override void goToPath(string remoteFolder)
    {
        if (FtpLogging.GoToFolder)
            OnNewStatus("Přecházím do složky" + " " + remoteFolder);
        var actualPath = PathSelector.ActualPath;
        var dd = remoteFolder.Length - 1;
        if (actualPath == remoteFolder)
            return;
        // Vzdálená složka začíná s aktuální cestou == vzdálená složka je delší. Pouze přejdi hloubš
        if (remoteFolder.StartsWith(actualPath))
        {
            remoteFolder = remoteFolder.Substring(actualPath.Length);
            var tokens = SHSplit.Split(remoteFolder, PathSelector.Delimiter);
            foreach (var item in tokens)
                CreateDirectoryIfNotExists(item);
        }
        // Vzdálená složka nezačíná aktuální cestou,
        else
        {
            PathSelector.ActualPath = "";
            var tokens = SHSplit.Split(remoteFolder, PathSelector.Delimiter);
            var pridat = 0;
            for (var i = 0 + pridat; i < tokens.Count; i++)
                CreateDirectoryIfNotExists(tokens[i]);
        }
    }

    /// <summary>
    ///     RENAME
    ///     Pošlu příkaz RNFR A1 a když bude odpoveď 350, tak RNTO
    /// </summary>
    /// <param name = "oldFileName"></param>
    /// <param name = "newFileName"></param>
    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        OnNewStatus("Ve složce" + " " + PathSelector.ActualPath + " " + "přejmenovávám soubor" + " " + oldFileName + " na " + newFileName);
        if (ExceptionCount < MaxExceptionCount)
        {
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(GetActualPath(oldFileName)));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFileName;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(remoteUser, remotePass);
                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();
            }
            catch (Exception ex)
            {
                OnNewStatus("Error rename file" + ": " + ex.Message);
                ExceptionCount++;
                renameRemoteFile(oldFileName, newFileName);
            }
            finally
            {
                if (ftpStream != null)
                    ftpStream.Dispose();
                if (response != null)
                    response.Dispose();
            }
        }

        ExceptionCount = 0;
    }

    /// <summary>
    /// Removes empty directory from FTP server using RMD command. Can only be called when directory is known to be empty, otherwise returns error 550.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during deletion</param>
    /// <param name="dirName">Directory name to remove</param>
    /// <returns>True if directory was removed successfully</returns>
    public override bool rmdir(List<string> foldersToSkip, string dirName)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            var ma = GetActualPath(dirName).TrimEnd('/');
            OnNewStatus("Mažu adresář" + " " + ma);
            FtpWebRequest clsRequest = null;
            StreamReader sr = null;
            Stream datastream = null;
            FtpWebResponse response = null;
            try
            {
                clsRequest = (FtpWebRequest)WebRequest.Create(new Uri(ma));
                clsRequest.Credentials = new NetworkCredential(remoteUser, remotePass);
                clsRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                var result = string.Empty;
                response = (FtpWebResponse)clsRequest.GetResponse();
                var size = response.ContentLength;
                datastream = response.GetResponseStream();
                sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                ExceptionCount++;
                if (sr != null)
                    sr.Dispose();
                if (datastream != null)
                    datastream.Dispose();
                if (response != null)
                    response.Dispose();
                OnNewStatus("Error delete folder" + ": " + ex.Message);
                return rmdir(foldersToSkip, dirName);
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
                if (datastream != null)
                    datastream.Dispose();
                if (response != null)
                    response.Dispose();
            }

            ExceptionCount = 0;
            return true;
        }

        ExceptionCount = 0;
        return false;
    }

    /// <summary>
    /// Recursively deletes directory and its contents using DELE and RMD commands
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during deletion</param>
    /// <param name="dirName">Root directory name to start deletion from</param>
    /// <param name="i">Current recursion depth level</param>
    /// <param name="td">List to collect directories marked for deletion</param>
    public override void DeleteRecursively(List<string> foldersToSkip, string dirName, int i, List<DirectoriesToDeleteFtp> td)
    {
        i++;
        var toDelete = ListDirectoryDetails();
        //bool pridano = false;
        td.Add(new DirectoriesToDeleteFtp { Depth = i });
        Dictionary<string, List<string>> ds = null;
        foreach (var item in td)
            if (item.Depth == i)
            {
                if (item.Directories.Count != 0)
                {
                    foreach (var item2 in item.Directories)
                        foreach (var item3 in item2)
                            if (item3.Key == PathSelector.ActualPath)
                                ds = item2;
                }
                else
                {
                    ds = new Dictionary<string, List<string>>();
                }
            //ds = ;
            }

        for (var itemIndex = 0; itemIndex < td.Count; itemIndex++)
        {
            var item = td[itemIndex];
            if (item.Depth == i)
                //ds.Add(PathSelector.ActualPath, new List<string>());
                foreach (var item2 in toDelete)
                {
                    var fn = "";
                    var fst = FtpHelper.IsFile(item2, out fn);
                    if (fst == FileSystemType.File)
                    {
                        if (ds.ContainsKey(PathSelector.ActualPath))
                        {
                        }
                        else
                        {
                            ds.Add(PathSelector.ActualPath, new List<string>());
                        }

                        var f = ds[PathSelector.ActualPath];
                        f.Add(fn);
                    }
                    else if (fst == FileSystemType.Folder)
                    {
                        PathSelector.AddToken(fn);
                        ds.Add(PathSelector.ActualPath, new List<string>());
                        //pridano = true;
                        DeleteRecursively(foldersToSkip, fn, i, td);
                    }
                ////DebugLogger.Instance.WriteLine(item2);
                }
        //item.Directories.Add(ds);
        }

        if (true)
            foreach (var item in td)
                if (item.Depth == i)
                    item.Directories.Add(ds);
        if (i == 1)
        {
            var smazaneAdresare = new List<string>();
            for (var yValue = td.Count - 1; yValue >= 0; yValue--)
                foreach (var item in td[yValue].Directories)
                    foreach (var item2 in item)
                    {
                        PathSelector.ActualPath = item2.Key;
                        var sa = item2.Key;
                        if (!smazaneAdresare.Contains(sa))
                        {
                            smazaneAdresare.Add(sa);
                            foreach (var item3 in item2.Value)
                                while (!deleteRemoteFile(item3))
                                {
                                }

                            goToUpFolderForce();
                            rmdir(new List<string>(), Path.GetFileName(item2.Key.TrimEnd('/')));
                        }
                    }
        }
    }
}