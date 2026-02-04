namespace SunamoFtp.Base;

public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    ///     IK, OOP.
    /// </summary>
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

    //public abstract void DeleteRecursively(List<string> foldersToSkip, string dirName, int i, List<DirectoriesToDelete> directoriesToDelete);
    /// <summary>
    /// Triggers status update notification for new folder navigation
    /// </summary>
    public void OnNewStatusNewFolder()
    {
        NewStatus("New folder is" + " " + PathSelector.ActualPath, []);
    }

    /// <summary>
    ///     Upload file by FtpWebRequest
    ///     OK
    ///     STOR
    ///     To upload a file to current folder and specify only file name on disk, use UploadFile method.
    /// </summary>
    /// <param name = "local"></param>
    /// <param name = "uploadPath"></param>
    public virtual bool UploadFileMain(string local, string uploadPath)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Uploading" + " " + uploadPath);
            var fileInfo = new FileInfo(local);
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
                return UploadFileMain(local, uploadPath);
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

    /// <summary>
    /// Triggers status update notification for file upload using safe method
    /// </summary>
    /// <param name="path">Path being uploaded</param>
    public void OnUploadingNewStatus(string path)
    {
        OnNewStatus("Uploading" + " " + path + " " + "using safe method");
    }

    /// <summary>
    /// Event raised when FTP operation status changes
    /// </summary>
    public static event Action<object, object[]> NewStatus;

    /// <summary>
    /// Raises the NewStatus event with specified message and parameters
    /// </summary>
    /// <param name="text">Status message</param>
    /// <param name="args">Additional parameters</param>
    public static void OnNewStatus(string text, params object[] args)
    {
        NewStatus(text, args);
    }

    /// <summary>
    /// Uploads only files that don't already exist in the current directory on FTP server
    /// </summary>
    /// <param name="files">List of local file paths to upload</param>
    /// <returns>True if all files were uploaded successfully</returns>
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

    /// <summary>
    /// Gets the current FTP path including host and port
    /// </summary>
    /// <returns>Full FTP path</returns>
    public string GetActualPath()
    {
        return UH.Combine(true, RemoteHost + ":" + RemotePort, PathSelector.ActualPath);
    }

    /// <summary>
    /// Gets the FTP path for specified directory/file name appended to current path
    /// </summary>
    /// <param name="dirName">Directory or file name (not full path)</param>
    /// <returns>Full FTP path including the specified name</returns>
    public string GetActualPath(string dirName)
    {
        var text = /*UH.Combine(true,*/ RemoteHost + ":" + RemotePort + PathSelector.ActualPath + dirName;
        return text.TrimEnd('/');
    }

    /// <summary>
    /// Uploads a local folder to FTP server. After calling this method in FTP class, you must call GoToUpFolder to return to previous directory.
    /// </summary>
    /// <param name="sourceFolder">Local source folder path</param>
    /// <param name="isFtpClass">Indicates if called from FTP class (requires GoToPath to restore)</param>
    /// <param name="working">Working state tracker to allow cancellation</param>
    /// <returns>True if folder was uploaded successfully</returns>
    public bool UploadFolder(string sourceFolder, bool isFtpClass, IWorking working)
    {
        var actPath = PathSelector.ActualPath;
        var result = UploadFolderShared(sourceFolder, false, working);
        if (isFtpClass)
            GoToPath(actPath);
        return result;
    }

    /// <summary>
    /// Recursively uploads a local folder and all its contents to specified remote folder
    /// </summary>
    /// <param name="localFolder">Local folder path to upload</param>
    /// <param name="remoteFolder">Remote FTP folder path to upload to</param>
    /// <returns>True if all files and folders were uploaded successfully</returns>
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

    /// <summary>
    /// Recursively uploads a local folder and all its contents to current FTP directory
    /// </summary>
    /// <param name="localFolder">Local folder path to upload</param>
    /// <param name="iw">Working state tracker to allow cancellation</param>
    /// <returns>True if all files and folders were uploaded successfully</returns>
    public bool UploadFolderRek(string localFolder, IWorking iw)
    {
        return UploadFolderShared(localFolder, true, iw);
    }
}