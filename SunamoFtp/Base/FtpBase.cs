namespace SunamoFtp.Base;

// EN: Variable names have been checked and replaced with self-descriptive names
// CZ: Názvy proměnných byly zkontrolovány a nahrazeny samopopisnými názvy
public abstract partial class FtpBase : FtpAbstract
{
    /// <summary>
    ///     IK, OOP.
    /// </summary>
    public FtpBase()
    {
        PathSelector = new PathSelector("");
        remoteHost = string.Empty;
        //remotePath = ".";
        remoteUser = string.Empty;
        remotePass = string.Empty;
        remotePort = 21;
        logined = false;
    }

    //public abstract void DeleteRecursively(List<string> foldersToSkip, string dirName, int i, List<DirectoriesToDelete> td);
    /// <summary>
    /// Triggers status update notification for new folder navigation
    /// </summary>
    public void OnNewStatusNewFolder()
    {
        NewStatus("Nová složka je" + " " + PathSelector.ActualPath, []);
    }

    /// <summary>
    ///     Upload file by FtpWebRequest
    ///     OK
    ///     STOR
    ///     Pokud chceš uploadovat soubor do aktuální složky a zvlolit pouze název souboru na disku, použij metodu UploadFile.
    /// </summary>
    /// <param name = "local"></param>
    /// <param name = "_UploadPath"></param>
    public virtual bool UploadFileMain(string local, string _UploadPath)
    {
        if (ExceptionCount < MaxExceptionCount)
        {
            OnNewStatus("Uploaduji" + " " + _UploadPath);
            var _FileInfo = new FileInfo(local);
            Stream _Stream = null;
            FileStream _FileStream = null;
            try
            {
                // Create FtpWebRequest object from the Uri provided
                var _FtpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(_UploadPath));
                // Provide the WebPermission Credintials
                _FtpWebRequest.Credentials = new NetworkCredential(remoteUser, remotePass);
                _FtpWebRequest.KeepAlive = false;
                // set timeout for 20 seconds
                _FtpWebRequest.Timeout = 20000;
                // Specify the command to be executed.
                _FtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                // Specify the data transfer type.
                _FtpWebRequest.UseBinary = true;
                // Notify the server about the size of the uploaded file
                _FtpWebRequest.ContentLength = _FileInfo.Length;
                // The buffer size is set to 2kb
                var buffLength = 2048;
                var buff = new byte[buffLength];
                // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
                _FileStream = _FileInfo.OpenRead();
                // Stream to which the file to be upload is written
                _Stream = _FtpWebRequest.GetRequestStream();
                // Read from the file stream 2kb at a time
                var contentLen = _FileStream.Read(buff, 0, buffLength);
                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    _Stream.Write(buff, 0, contentLen);
                    contentLen = _FileStream.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                _Stream.Close();
                _Stream.Dispose();
                _FileStream.Close();
                _FileStream.Dispose();
                ExceptionCount = 0;
            // Close the file stream and the Request Stream
            }
            catch (Exception ex)
            {
                ExceptionCount++;
                //CleanUp.Streams(_Stream, _FileStream);
                _Stream.Dispose();
                _FileStream.Dispose();
                OnNewStatus("Upload file error" + ": " + ex.Message);
                return UploadFileMain(local, _UploadPath);
            }
            finally
            {
                //CleanUp.Streams(_Stream, _FileStream);
                _Stream.Dispose();
                _FileStream.Dispose();
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
        OnNewStatus("Uploaduji" + " " + path + " " + "bezpečnou metodou");
    }

    /// <summary>
    /// Event raised when FTP operation status changes
    /// </summary>
    public static event Action<object, object[]> NewStatus;

    /// <summary>
    /// Raises the NewStatus event with specified message and parameters
    /// </summary>
    /// <param name="text">Status message</param>
    /// <param name="p">Additional parameters</param>
    public static void OnNewStatus(string text, params object[] p)
    {
        NewStatus(text, p);
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
        return UH.Combine(true, remoteHost + ":" + remotePort, PathSelector.ActualPath);
    }

    /// <summary>
    /// Gets the FTP path for specified directory/file name appended to current path
    /// </summary>
    /// <param name="dirName">Directory or file name (not full path)</param>
    /// <returns>Full FTP path including the specified name</returns>
    public string GetActualPath(string dirName)
    {
        var text = /*UH.Combine(true,*/ remoteHost + ":" + remotePort + PathSelector.ActualPath + dirName;
        return text.TrimEnd('/');
    }

    /// <summary>
    /// Uploads a local folder to FTP server. After calling this method in FTP class, you must call goToUpFolder to return to previous directory.
    /// </summary>
    /// <param name="sourceFolder">Local source folder path</param>
    /// <param name="FTPclass">Indicates if called from FTP class (requires goToPath to restore)</param>
    /// <param name="working">Working state tracker to allow cancellation</param>
    /// <returns>True if folder was uploaded successfully</returns>
    public bool uploadFolder(string sourceFolder, bool FTPclass, IWorking working)
    {
        var actPath = PathSelector.ActualPath;
        var result = uploadFolderShared(sourceFolder, false, working);
        if (FTPclass)
            goToPath(actPath);
        return result;
    }

    /// <summary>
    /// Recursively uploads a local folder and all its contents to specified remote folder
    /// </summary>
    /// <param name="localFolder">Local folder path to upload</param>
    /// <param name="remoteFolder">Remote FTP folder path to upload to</param>
    /// <returns>True if all files and folders were uploaded successfully</returns>
    public bool uploadFolderRek(string localFolder, string remoteFolder)
    {
        // Musí to tu být právě kvůli předchozímu řádku List<string> ftpEntries = getFSEntriesList(); kdy získávám seznam souborů na FTP serveru
        goToPath(remoteFolder);
        var directories = Directory.GetDirectories(localFolder);
        var files = Directory.GetFiles(localFolder).ToList();
        OnNewStatus("Uploaduji všechny files" + " " + files.Count() + " " + "do složky ftp serveru" + " " + PathSelector.ActualPath);
        if (!UploadFiles(files))
            return false;
        foreach (var item in directories)
            if (!uploadFolderRek(item, UH.Combine(false, remoteFolder, Path.GetFileName(item))))
                return false;
        return true;
    }

    /// <summary>
    /// Recursively uploads a local folder and all its contents to current FTP directory
    /// </summary>
    /// <param name="localFolder">Local folder path to upload</param>
    /// <param name="iw">Working state tracker to allow cancellation</param>
    /// <returns>True if all files and folders were uploaded successfully</returns>
    public bool uploadFolderRek(string localFolder, IWorking iw)
    {
        return uploadFolderShared(localFolder, true, iw);
    }
}