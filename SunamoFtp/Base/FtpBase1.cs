namespace SunamoFtp.Base;

public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    /// Recursively gets all filesystem entries from FTP server starting from specified folder. This is an internal method - call the 1-parameter overload instead.
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during traversal</param>
    /// <param name="visitedFolders">List of already visited folder paths to avoid infinite loops</param>
    /// <param name="result">Dictionary to collect filesystem entries mapped by directory path</param>
    /// <param name="folderName">Folder name to start traversal from</param>
    public void GetFSEntriesListRecursively(List<string> foldersToSkip, List<string> visitedFolders, Dictionary<string, List<string>> result, string folderName)
    {
        LoginIfIsNot(IsStartup);
        var nextPath = UH.Combine(true, PathSelector.ActualPath, folderName);
        if (!visitedFolders.Contains(nextPath))
        {
            NewStatus("Navigating to folder" + " " + nextPath + " " + "which has not been visited yet", []);
            PathSelector.AddToken(folderName);
            visitedFolders.Add(nextPath);
            var ftpEntries = ListDirectoryDetails();
            var actualPath = PathSelector.ActualPath;
            foreach (var item in ftpEntries)
            {
                var size = SHJoin.JoinFromIndex(4, ' ', item.Split(' ').ToList());
                var firstChar = item[0];
                if (firstChar == '-')
                {
                    if (size != "0")
                        FolderSizeRecursive += ulong.Parse(size.Substring(0, size.IndexOf(' ') + 1));
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
                    throw new Exception("Unsupported object type");
                }
            }

            if (PathSelector.CanGoToUpFolder)
                GoToUpFolder();
        //PathSelector.RemoveLastToken();
        }
        else
        {
            NewStatus("Folder" + " " + nextPath + " " + "has already been visited", []);
        }
    //PathSelector.ActualPath = p;
    }

    /// <summary>
    /// Downloads a file from FTP server to local filesystem (deletes local file if exists)
    /// </summary>
    /// <param name="remFileName">Remote file name on FTP server</param>
    /// <param name="locFileName">Local file path to save to</param>
    public void Download(string remFileName, string locFileName)
    {
        Download(remFileName, locFileName, true);
    }

    /// <summary>
    /// Uploads file to current FTP directory. You must navigate to target folder before calling this method.
    /// </summary>
    /// <param name="fileName">Local source file path</param>
    public void UploadFile(string fileName)
    {
        var uploadPath = UH.Combine(false, RemoteHost + ":" + RemotePort + "/", UH.Combine(true, PathSelector.ActualPath, Path.GetFileName(fileName)));
        if (ReallyUpload)
            UploadFileMain(fileName, uploadPath);
    //MainWindow.FileUploaded(fileName);
    }

    /// <summary>
    /// Uploads file to specified FTP folder path (allows uploading to different folder than current)
    /// </summary>
    /// <param name="fullFilePath">Local file path to upload</param>
    /// <param name="actualFtpPath">Target FTP folder path</param>
    /// <returns>True if file was uploaded successfully</returns>
    public bool UploadFile(string fullFilePath, string actualFtpPath)
    {
        var uploadPath = UH.Combine(false, RemoteHost + ":" + RemotePort + "/" + "/", UH.Combine(false, actualFtpPath, Path.GetFileName(fullFilePath)));
        var result = true;
        if (ReallyUpload)
            result = UploadFileMain(fullFilePath, uploadPath);
        return result;
    }

    /// <summary>
    /// Shared method for uploading folder to FTP server (used by both recursive and non-recursive variants)
    /// </summary>
    /// <param name="sourceFolder">Local source folder path</param>
    /// <param name="isRecursive">Whether to recursively upload subfolders</param>
    /// <param name="working">Working state tracker to allow cancellation</param>
    /// <returns>True if folder was uploaded successfully</returns>
    public bool UploadFolderShared(string sourceFolder, bool isRecursive, IWorking working)
    {
        var folderName = Path.GetFileName(sourceFolder);
        var pathFolder = UH.Combine(true, PathSelector.ActualPath, folderName);
        sourceFolder = sourceFolder.TrimEnd('\\');
        var files = Directory.GetFiles(sourceFolder).ToList();
        var folders = Directory.GetDirectories(sourceFolder);
        NewStatus("Uploading all files" + " " + files.Count() + " " + "to FTP server folder" + " " + pathFolder, []);
        CreateDirectoryIfNotExists(folderName);
        foreach (var item in files)
        {
            if (!working.IsWorking)
                return false;
            UploadFile(item);
        }

        if (isRecursive)
        {
            if (folders.Count() == 0)
            {
                GoToUpFolder();
            }
            else
            {
                foreach (var item in folders)
                    UploadFolderShared(item, isRecursive, working);
                if (folders.Count() != 0)
                    GoToUpFolder();
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