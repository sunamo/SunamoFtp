namespace SunamoFtp.Base;

public abstract partial class FtpBase : FtpAbstract
{
    // Internal method - call the 1-parameter overload instead.
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
                    //getFSEntriesListRecursively(foldersToSkip, visitedFolders, result, PathSelector.ActualPath,extractedFolderName);
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

    public void Download(string remFileName, string locFileName)
    {
        Download(remFileName, locFileName, true);
    }

    // You must navigate to target folder before calling this method.
    public void UploadFile(string filePath)
    {
        var uploadPath = UH.Combine(false, RemoteHost + ":" + RemotePort + "/", UH.Combine(true, PathSelector.ActualPath, Path.GetFileName(filePath)));
        if (ReallyUpload)
            UploadFileMain(filePath, uploadPath);
    //MainWindow.FileUploaded(fileName);
    }

    public bool UploadFile(string filePath, string actualFtpPath)
    {
        var uploadPath = UH.Combine(false, RemoteHost + ":" + RemotePort + "/" + "/", UH.Combine(false, actualFtpPath, Path.GetFileName(filePath)));
        var result = true;
        if (ReallyUpload)
            result = UploadFileMain(filePath, uploadPath);
        return result;
    }

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

    public bool ExistsFolder(string folder)
    {
        var ftpEntries = ListDirectoryDetails();
        var data = new List<string>(FtpHelper.GetDirectories(ftpEntries));
        return data.Contains(folder);
    }
}
