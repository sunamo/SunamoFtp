namespace SunamoFtp.Base;

public abstract partial class FtpBase : FtpAbstract
{
    public FtpBase()
    {
        PathSelector = new PathSelector("");
        RemoteHost = string.Empty;
        //remotePath = ".";
        RemoteUser = string.Empty;
        RemotePass = string.Empty;
        RemotePort = 21;
        IsLoggedIn = false;
    }

    //public abstract void DeleteRecursively(List<string> foldersToSkip, string directoryName, int i, List<DirectoriesToDelete> directoriesToDelete);
    public void OnNewStatusNewFolder()
    {
        NewStatus("New folder is" + " " + PathSelector.ActualPath, []);
    }

    public virtual bool UploadFileMain(string path, string uploadPath)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Uploading" + " " + uploadPath);
            var fileInfo = new FileInfo(path);
            Stream ftpStream = null;
            FileStream fileStream = null;
            try
            {
                // Create FtpWebRequest object from the Uri provided
                var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(uploadPath));
                // Provide the WebPermission Credintials
                ftpWebRequest.Credentials = new NetworkCredential(RemoteUser, RemotePass);
                ftpWebRequest.KeepAlive = false;
                // set timeout for 20 seconds
                ftpWebRequest.Timeout = 20000;
                // Specify the command to be executed.
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                // Specify the data transfer type.
                ftpWebRequest.UseBinary = true;
                // Notify the server about the size of the uploaded file
                ftpWebRequest.ContentLength = fileInfo.Length;
                // The buffer size is set to 2kb
                var buffLength = 2048;
                var buffer = new byte[buffLength];
                // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
                fileStream = fileInfo.OpenRead();
                // Stream to which the file to be upload is written
                ftpStream = ftpWebRequest.GetRequestStream();
                // Read from the file stream 2kb at a time
                var contentLen = fileStream.Read(buffer, 0, buffLength);
                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    ftpStream.Write(buffer, 0, contentLen);
                    contentLen = fileStream.Read(buffer, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                ftpStream.Close();
                ftpStream.Dispose();
                fileStream.Close();
                fileStream.Dispose();
                ExceptionCount = 0;
            // Close the file stream and the Request Stream
            }
            catch (Exception ex)
            {
                ExceptionCount++;
                //CleanUp.Streams(ftpStream, fileStream);
                ftpStream.Dispose();
                fileStream.Dispose();
                OnNewStatus("Upload file error" + ": " + ex.Message);
                return UploadFileMain(path, uploadPath);
            }
            finally
            {
                //CleanUp.Streams(ftpStream, fileStream);
                ftpStream.Dispose();
                fileStream.Dispose();
            }

            ExceptionCount = 0;
            return true;
        }

        ExceptionCount = 0;
        return false;
    }

    public void OnUploadingNewStatus(string path)
    {
        OnNewStatus("Uploading" + " " + path + " " + "using safe method");
    }

    public static event Action<object, object[]> NewStatus;

    public static void OnNewStatus(string text, params object[] args)
    {
        NewStatus(text, args);
    }

    public bool UploadFiles(List<string> files)
    {
        var ftpEntries = ListDirectoryDetails();
        foreach (var item in files)
        {
            var fi = new FileInfo(item);
            var fileSize = fi.Length;
            if (!FtpHelper.IsFileOnHosting(item, ftpEntries, fileSize))
                UploadFile(item);
        }

        return true;
    }

    public string GetActualPath() => UH.Combine(true, RemoteHost + ":" + RemotePort, PathSelector.ActualPath);

    public string GetActualPath(string directoryName)
    {
        var text = /*UH.Combine(true,*/ RemoteHost + ":" + RemotePort + PathSelector.ActualPath + directoryName;
        return text.TrimEnd('/');
    }

    public bool UploadFolder(string localFolder, bool isFtpClass, IWorking working)
    {
        var actPath = PathSelector.ActualPath;
        var result = UploadFolderShared(localFolder, false, working);
        if (isFtpClass)
            GoToPath(actPath);
        return result;
    }

    public bool UploadFolderRek(string localFolder, string remoteFolder)
    {
        // This is required due to previous line where we get file list from FTP server
        GoToPath(remoteFolder);
        var directories = Directory.GetDirectories(localFolder);
        var files = Directory.GetFiles(localFolder).ToList();
        OnNewStatus("Uploading all files" + " " + files.Count() + " " + "to FTP server folder" + " " + PathSelector.ActualPath);
        if (!UploadFiles(files))
            return false;
        foreach (var item in directories)
            if (!UploadFolderRek(item, UH.Combine(false, remoteFolder, Path.GetFileName(item))))
                return false;
        return true;
    }

    public bool UploadFolderRek(string localFolder, IWorking working) => UploadFolderShared(localFolder, true, working);
}
