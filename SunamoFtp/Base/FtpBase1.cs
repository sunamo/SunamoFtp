namespace SunamoFtp.Base;

public abstract partial class FtpBase : FtpAbstract
{
    // Internal method - call the 1-parameter overload instead.
    /// <summary>
    /// Get FS entries list recursively.
    /// </summary>
    /// <param name="foldersToSkip">The folders to skip.</param>
    /// <param name="visitedFolders">The visited folders.</param>
    /// <param name="result">The result.</param>
    /// <param name="folderName">The folder name.</param>
    public void GetFSEntriesListRecursively(List<string> foldersToSkip, List<string> visitedFolders, Dictionary<string, List<string>> result, string folderName)
    {
        LoginIfIsNot(IsInitialLogin);
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
                    var extractedFolderName = SHJoin.JoinFromIndex(8, ' ', item.Split(' '));
                    if (!FtpHelper.IsThisOrUp(extractedFolderName))
                    {
                        if (foldersToSkip.Contains(extractedFolderName) && PathSelector.ActualPath == MainWindow.WwwSlash)
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
                    }
                }
                else
                {
                    throw new Exception("Unsupported object type");
                }
            }

            if (PathSelector.CanGoToUpFolder)
                GoToUpFolder();
        }
        else
        {
            NewStatus("Folder" + " " + nextPath + " " + "has already been visited", []);
        }
    }

    /// <summary>
    /// Download.
    /// </summary>
    /// <param name="remFileName">The rem file name.</param>
    /// <param name="locFileName">The loc file name.</param>
    public void Download(string remFileName, string locFileName)
    {
        Download(remFileName, locFileName, true);
    }

    // You must navigate to target folder before calling this method.
    /// <summary>
    /// Upload file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public void UploadFile(string filePath)
    {
        var uploadPath = UH.Combine(false, RemoteHost + ":" + RemotePort + "/", UH.Combine(true, PathSelector.ActualPath, Path.GetFileName(filePath)));
        if (ReallyUpload)
            UploadFileMain(filePath, uploadPath);
    }

    /// <summary>
    /// Upload file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="actualFtpPath">The actual ftp path.</param>
    /// <returns>The upload file result.</returns>
    public bool UploadFile(string filePath, string actualFtpPath)
    {
        var uploadPath = UH.Combine(false, RemoteHost + ":" + RemotePort + "/" + "/", UH.Combine(false, actualFtpPath, Path.GetFileName(filePath)));
        var result = true;
        if (ReallyUpload)
            result = UploadFileMain(filePath, uploadPath);
        return result;
    }

    /// <summary>
    /// Upload folder shared.
    /// </summary>
    /// <param name="sourceFolder">The source folder.</param>
    /// <param name="isRecursive">The is recursive.</param>
    /// <param name="working">The working.</param>
    /// <returns>The upload folder shared result.</returns>
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
    /// Exists folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <returns>The exists folder result.</returns>
    public bool ExistsFolder(string folder)
    {
        var ftpEntries = ListDirectoryDetails();
        var data = new List<string>(FtpHelper.GetDirectories(ftpEntries));
        return data.Contains(folder);
    }
}
