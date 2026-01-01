namespace SunamoFtp.Base;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    /// Recursively gets all filesystem entries from FTP server starting from specified folder. This is an internal method - call the 1-parameter overload instead.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during traversal</param>
    /// <param name="visitedFolders">List of already visited folder paths to avoid infinite loops</param>
    /// <param name="result">Dictionary to collect filesystem entries mapped by directory path</param>
    /// <param name="folderName">Folder name to start traversal from</param>
    public void getFSEntriesListRecursively(List<string> foldersToSkip, List<string> visitedFolders, Dictionary<string, List<string>> result, string folderName)
    {
        LoginIfIsNot(startup);
        var nextPath = UH.Combine(true, PathSelector.ActualPath, folderName);
        if (!visitedFolders.Contains(nextPath))
        {
            NewStatus("Složka do které se mělo přejít" + " " + nextPath + " " + "ještě nebyla v projeté kolekci", []);
            PathSelector.AddToken(folderName);
            visitedFolders.Add(nextPath);
            var ftpEntries = ListDirectoryDetails();
            var actualPath = PathSelector.ActualPath;
            foreach (var item in ftpEntries)
            {
                var size = SHJoin.JoinFromIndex(4, ' ', item.Split(' ').ToList());
                var fz = item[0];
                if (fz == '-')
                {
                    if (size != "0")
                        folderSizeRec += ulong.Parse(size.Substring(0, size.IndexOf(' ') + 1));
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
                    var folderName2 = SHJoin.JoinFromIndex(8, ' ', item.Split(' '));
                    if (!FtpHelper.IsThisOrUp(folderName2))
                    {
                        if (foldersToSkip.Contains(folderName2) && PathSelector.ActualPath == MainWindow.WwwSlash)
                            continue;
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
                    //getFSEntriesListRecursively(foldersToSkip, visitedFolders, result, PathSelector.ActualPath,folderName2);
                    }
                }
                else
                {
                    throw new Exception("Nepodporovaný typ objektu");
                }
            }

            if (PathSelector.CanGoToUpFolder)
                goToUpFolder();
        //PathSelector.RemoveLastToken();
        }
        else
        {
            NewStatus("Složka do které se mělo přejít" + " " + nextPath + " " + "již byla v projeté kolekci", []);
        }
    //PathSelector.ActualPath = p;
    }

    /// <summary>
    /// Downloads a file from FTP server to local filesystem (deletes local file if exists)
    /// </summary>
    /// <param name="remFileName">Remote file name on FTP server</param>
    /// <param name="locFileName">Local file path to save to</param>
    public void download(string remFileName, string locFileName)
    {
        download(remFileName, locFileName, true);
    }

    /// <summary>
    /// Uploads file to current FTP directory. You must navigate to target folder before calling this method.
    /// </summary>
    /// <param name="_FileName">Local source file path</param>
    public void UploadFile(string _FileName)
    {
        var _UploadPath = UH.Combine(false, remoteHost + ":" + remotePort + "/", UH.Combine(true, PathSelector.ActualPath, Path.GetFileName(_FileName)));
        if (reallyUpload)
            UploadFileMain(_FileName, _UploadPath);
    //MainWindow.FileUploaded(_FileName);
    }

    /// <summary>
    /// Uploads file to specified FTP folder path (allows uploading to different folder than current)
    /// </summary>
    /// <param name="fullFilePath">Local file path to upload</param>
    /// <param name="actualFtpPath">Target FTP folder path</param>
    /// <returns>True if file was uploaded successfully</returns>
    public bool UploadFile(string fullFilePath, string actualFtpPath)
    {
        var _UploadPath = UH.Combine(false, remoteHost + ":" + remotePort + "/" + "/", UH.Combine(false, actualFtpPath, Path.GetFileName(fullFilePath)));
        var result = true;
        if (reallyUpload)
            result = UploadFileMain(fullFilePath, _UploadPath);
        return result;
    }

    /// <summary>
    /// Shared method for uploading folder to FTP server (used by both recursive and non-recursive variants)
    /// </summary>
    /// <param name="sourceFolder">Local source folder path</param>
    /// <param name="rek">Whether to recursively upload subfolders</param>
    /// <param name="working">Working state tracker to allow cancellation</param>
    /// <returns>True if folder was uploaded successfully</returns>
    public bool uploadFolderShared(string sourceFolder, bool rek, IWorking working)
    {
        var folderName = Path.GetFileName(sourceFolder);
        var pathFolder = UH.Combine(true, PathSelector.ActualPath, folderName);
        sourceFolder = sourceFolder.TrimEnd('\\');
        var files = Directory.GetFiles(sourceFolder).ToList();
        var folders = Directory.GetDirectories(sourceFolder);
        NewStatus("Uploaduji všechny files" + " " + files.Count() + " " + "do složky ftp serveru" + " " + pathFolder, []);
        CreateDirectoryIfNotExists(folderName);
        foreach (var item in files)
        {
            if (!working.IsWorking)
                return false;
            UploadFile(item);
        }

        if (rek)
        {
            if (folders.Count() == 0)
            {
                goToUpFolder();
            }
            else
            {
                foreach (var item in folders)
                    uploadFolderShared(item, rek, working);
                if (folders.Count() != 0)
                    goToUpFolder();
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if folder exists in current FTP directory
    /// </summary>
    /// <param name="folder">Folder name (without path)</param>
    /// <returns>True if folder exists in current directory</returns>
    public bool ExistsFolder(string folder)
    {
        var ftpEntries = ListDirectoryDetails();
        var data = new List<string>(FtpHelper.GetDirectories(ftpEntries));
        return data.Contains(folder);
    }
}