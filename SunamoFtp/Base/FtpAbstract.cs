namespace SunamoFtp.Base;

public abstract class FtpAbstract
{
    public abstract void Connect();

    public abstract void WriteDebugLog(string context, string text, params object[] args);

    public abstract void DebugActualFolder();

    #region Variables

    public IFtpClientExt MainWindow { get; set; } = null;

    // Public only for Ftp class.
    public PathSelector PathSelector { get; set; } = null;

    public string RemoteHost { get; set; }

    public string RemoteUser { get; set; }

    public string RemotePass { get; set; }

    public int RemotePort { get; set; }

    public bool IsLoggedIn { get; set; }

    // If set to false, nothing will be uploaded to hosting. Used only in this class, everything else will work normally.
    public bool ReallyUpload { get; set; } = true;

    // Number of exceptions for single operation. Ideal for counting up to 3 and then canceling entire operation.
    protected int ExceptionCount { get; set; } = 0;

    protected int MaxExceptionCount { get; set; } = 3;

    protected bool IsInitialLogin { get; set; } = true;

    public ulong FolderSizeRecursive { get; set; } = 0;

    #endregion

    #region Set variables methods

    public void SetRemoteHost(string remoteHost)
    {
        RemoteHost = remoteHost;
    }

    public string GetRemoteHost() => RemoteHost;

    public void SetRemotePort(int remotePort)
    {
        RemotePort = remotePort;
    }

    public int GetRemotePort() => RemotePort;

    public void SetRemoteUser(string remoteUser)
    {
        RemoteUser = remoteUser;
    }

    public void SetRemotePass(string remotePass)
    {
        RemotePass = remotePass;
    }

    #endregion

    #region Abstract methods

    public abstract bool Mkdir(string directoryName);

    public abstract bool Download(string remFileName, string locFileName, bool isDeleteLocalIfExists);

    public abstract bool DeleteRemoteFile(string fileName);

    public abstract void RenameRemoteFile(string oldFileName, string newFileName);

    public abstract bool Rmdir(List<string> foldersToSkip, string directoryName);

    public abstract void DeleteRecursively(List<string> foldersToSkip, string directoryName, int depth,
        List<DirectoriesToDeleteFtp> directoriesToDelete);

    public abstract void CreateDirectoryIfNotExists(string directoryName);

    public abstract List<string> ListDirectoryDetails();

    public abstract Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip);

    public abstract void ChdirLite(string directoryName);

    public abstract void GoToUpFolderForce();

    public abstract void GoToUpFolder();

    public abstract void LoginIfIsNot(bool isInitialLogin);

    public abstract long GetFileSize(string filename);

    public abstract void GoToPath(string remoteFolder);

    #endregion
}
