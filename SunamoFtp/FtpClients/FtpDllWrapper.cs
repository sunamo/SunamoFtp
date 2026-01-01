namespace SunamoFtp.FtpClients;

/// <summary>
/// Wrapper around Ftp.dll library FTP client
/// </summary>
public class FtpDllWrapper : FtpBaseNew
{
    /// <summary>
    /// Underlying Ftp.dll client instance
    /// </summary>
    public Ftp Client;

    /// <summary>
    /// Initializes wrapper with Ftp.dll client instance
    /// </summary>
    /// <param name="ftp">Ftp.dll client to wrap</param>
    public FtpDllWrapper(Ftp ftp)
    {
        Client = ftp;
    }

    public override void chdirLite(string dirName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void CreateDirectoryIfNotExists(string dirName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void D(string what, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void DebugActualFolder()
    {
        //InitApp.Logger.WriteLine("Actual dir" + ":", Client.GetCurrentFolder());
    }

    public override void DebugAllEntries()
    {
        //InitApp.Logger.WriteLine("All file entries" + ":");
        //Client.GetList().ForEach(d => InitApp.Logger.WriteLine(d.Name));
    }

    public override void DebugDirChmod(string dir)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void DeleteRecursively(List<string> foldersToSkip, string dirName, int i,
        List<DirectoriesToDeleteFtp> td)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool deleteRemoteFile(string fileName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override bool download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override long getFileSize(string filename)
    {
        ThrowEx.NotImplementedMethod();
        return 0;
    }

    public override Dictionary<string, List<string>> getFSEntriesListRecursively(List<string> foldersToSkip)
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    public override void goToPath(string remoteFolder)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void goToUpFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override void goToUpFolderForce()
    {
        ThrowEx.NotImplementedMethod();
    }

    public override List<string> ListDirectoryDetails()
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    public override void LoginIfIsNot(bool startup)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool mkdir(string dirName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    public override void renameRemoteFile(string oldFileName, string newFileName)
    {
        ThrowEx.NotImplementedMethod();
    }

    public override bool rmdir(List<string> foldersToSkip, string dirName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    /// <summary>
    /// Uploads file to FTP server (not implemented)
    /// </summary>
    /// <param name="path">File path to upload</param>
    public override
#if ASYNC
        async Task
#else
void
#endif
        UploadFile(string path)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Disposes FTP client resources (not implemented)
    /// </summary>
    public override void Dispose()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Connects to FTP server (not implemented)
    /// </summary>
    public override void Connect()
    {
        ThrowEx.NotImplementedMethod();
    }
}