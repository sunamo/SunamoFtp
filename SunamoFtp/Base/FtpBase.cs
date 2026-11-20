namespace SunamoFtp.Base;

/// <summary>
/// Represents the ftp base type.
/// </summary>
public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    /// Initializes a new instance of the ftp base class.
    /// </summary>
    public FtpBase()
    {
        PathSelector = new PathSelector("");
        RemoteHost = string.Empty;
        RemoteUser = string.Empty;
        RemotePass = string.Empty;
        RemotePort = 21;
        IsLoggedIn = false;
    }

    /// <summary>
    /// On new status new folder.
    /// </summary>
    public void OnNewStatusNewFolder()
    {
        NewStatus("New folder is" + " " + PathSelector.ActualPath, []);
    }

    /// <summary>
    /// Upload file main.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="uploadPath">The upload path.</param>
    /// <returns>The upload file main result.</returns>
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
                ftpStream.Dispose();
                fileStream.Dispose();
                OnNewStatus("Upload file error" + ": " + ex.Message);
                return UploadFileMain(path, uploadPath);
            }
            finally
            {
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
    /// On uploading new status.
    /// </summary>
    /// <param name="path">The path.</param>
    public void OnUploadingNewStatus(string path)
    {
        OnNewStatus("Uploading" + " " + path + " " + "using safe method");
    }

    /// <summary>
    /// The new status.
    /// </summary>
    public static event Action<object, object[]> NewStatus;

    /// <summary>
    /// On new status.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="args">The args.</param>
    /// <returns>The on new status result.</returns>
    public static void OnNewStatus(string text, params object[] args)
    {
        NewStatus(text, args);
    }

    /// <summary>
    /// Initializes a new instance of the upload files class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="args">The args.</param>
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
    public string GetActualPath() => UH.Combine(true, RemoteHost + ":" + RemotePort, PathSelector.ActualPath);

    /// <summary>
    /// Get actual path.
    /// </summary>
    /// <returns>The get actual path result.</returns>
    public string GetActualPath(string directoryName)
    {
        var text = /*UH.Combine(true,*/ RemoteHost + ":" + RemotePort + PathSelector.ActualPath + directoryName;
        return text.TrimEnd('/');
    }

    /// <summary>
    /// Upload folder.
    /// </summary>
    /// <param name="'/'">The '/'.</param>
    /// <returns>The upload folder result.</returns>
    public bool UploadFolder(string localFolder, bool isFtpClass, IWorking working)
    {
        var actPath = PathSelector.ActualPath;
        var result = UploadFolderShared(localFolder, false, working);
        if (isFtpClass)
            GoToPath(actPath);
        return result;
    }

    /// <summary>
    /// Upload folder rek.
    /// </summary>
    /// <param name="localFolder">The local folder.</param>
    /// <param name="remoteFolder">The remote folder.</param>
    /// <returns>The upload folder rek result.</returns>
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
    /// <param name="working">Working state tracker to allow cancellation</param>
    /// <returns>True if all files and folders were uploaded successfully</returns>
    public bool UploadFolderRek(string localFolder, IWorking working) => UploadFolderShared(localFolder, true, working);
}