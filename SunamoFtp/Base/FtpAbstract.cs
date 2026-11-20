namespace SunamoFtp.Base;

/// <summary>
/// Represents the ftp abstract type.
/// </summary>
public abstract class FtpAbstract
{
    /// <summary>
    /// Connect.
    /// </summary>
    public abstract void Connect();

    /// <summary>
    /// Write debug log.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="text">The text.</param>
    /// <param name="args">The args.</param>
    public abstract void WriteDebugLog(string context, string text, params object[] args);

    /// <summary>
    /// Debug actual folder.
    /// </summary>
    public abstract void DebugActualFolder();

    #region Variables

    /// <summary>
    /// Gets or sets the main window.
    /// </summary>
    public IFtpClientExt MainWindow { get; set; } = null;

    // Public only for Ftp class.
    /// <summary>
    /// Gets or sets the path selector.
    /// </summary>
    public PathSelector PathSelector { get; set; } = null;

    /// <summary>
    /// Gets or sets the remote host.
    /// </summary>
    public string RemoteHost { get; set; }

    /// <summary>
    /// Gets or sets the remote user.
    /// </summary>
    public string RemoteUser { get; set; }

    /// <summary>
    /// Gets or sets the remote pass.
    /// </summary>
    public string RemotePass { get; set; }

    /// <summary>
    /// Gets or sets the remote port.
    /// </summary>
    public int RemotePort { get; set; }

    /// <summary>
    /// Gets or sets the is logged in.
    /// </summary>
    public bool IsLoggedIn { get; set; }

    // If set to false, nothing will be uploaded to hosting. Used only in this class, everything else will work normally.
    /// <summary>
    /// Gets or sets the really upload.
    /// </summary>
    public bool ReallyUpload { get; set; } = true;

    // Number of exceptions for single operation. Ideal for counting up to 3 and then canceling entire operation.
    /// <summary>
    /// Gets or sets the exception count.
    /// </summary>
    protected int ExceptionCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the max exception count.
    /// </summary>
    protected int MaxExceptionCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the is initial login.
    /// </summary>
    protected bool IsInitialLogin { get; set; } = true;

    /// <summary>
    /// Gets or sets the folder size recursive.
    /// </summary>
    public ulong FolderSizeRecursive { get; set; } = 0;

    #endregion

    #region Set variables methods

    /// <summary>
    /// Set remote host.
    /// </summary>
    /// <param name="remoteHost">The remote host.</param>
    public void SetRemoteHost(string remoteHost)
    {
        RemoteHost = remoteHost;
    }

    /// <summary>
    /// Gets remote host address
    /// </summary>
    /// <returns>Remote host address</returns>
    public string GetRemoteHost() => RemoteHost;

    /// <summary>
    /// Set remote port.
    /// </summary>
    /// <param name="remotePort">The remote port.</param>
    public void SetRemotePort(int remotePort)
    {
        RemotePort = remotePort;
    }

    /// <summary>
    /// Gets port used for remote transfer
    /// </summary>
    /// <returns>Port number</returns>
    public int GetRemotePort() => RemotePort;

    /// <summary>
    /// Set remote user.
    /// </summary>
    /// <param name="remoteUser">The remote user.</param>
    public void SetRemoteUser(string remoteUser)
    {
        RemoteUser = remoteUser;
    }

    /// <summary>
    /// Set remote pass.
    /// </summary>
    /// <param name="remoteHost">The remote host.</param>
    /// <returns>The set remote pass result.</returns>
    public void SetRemotePass(string remotePass)
    {
        RemotePass = remotePass;
    }

    #endregion

    #region Abstract methods

    /// <summary>
    /// Mkdir.
    /// </summary>
    /// <param name="remoteHost">The remote host.</param>
    public abstract bool Mkdir(string directoryName);

    /// <summary>
    /// Download.
    /// </summary>
    /// <returns>The download result.</returns>
    public abstract bool Download(string remFileName, string locFileName, bool isDeleteLocalIfExists);

    /// <summary>
    /// Delete remote file.
    /// </summary>
    /// <returns>The delete remote file result.</returns>
    public abstract bool DeleteRemoteFile(string fileName);

    /// <summary>
    /// Rename remote file.
    /// </summary>
    /// <returns>The rename remote file result.</returns>
    public abstract void RenameRemoteFile(string oldFileName, string newFileName);

    /// <summary>
    /// Rmdir.
    /// </summary>
    /// <returns>The rmdir result.</returns>
    public abstract bool Rmdir(List<string> foldersToSkip, string directoryName);

    /// <summary>
    /// Delete recursively.
    /// </summary>
    /// <param name="remotePort">The remote port.</param>
    public abstract void DeleteRecursively(List<string> foldersToSkip, string directoryName, int depth,
        List<DirectoriesToDeleteFtp> directoriesToDelete);

    /// <summary>
    /// Create directory if not exists.
    /// </summary>
    /// <returns>The create directory if not exists result.</returns>
    public abstract void CreateDirectoryIfNotExists(string directoryName);

    /// <summary>
    /// List directory details.
    /// </summary>
    /// <returns>The list directory details result.</returns>
    public abstract List<string> ListDirectoryDetails();

    /// <summary>
    /// Get FS entries list recursively.
    /// </summary>
    /// <returns>The get FS entries list recursively result.</returns>
    public abstract Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip);

    /// <summary>
    /// Chdir lite.
    /// </summary>
    /// <returns>The chdir lite result.</returns>
    public abstract void ChdirLite(string directoryName);

    /// <summary>
    /// Go to up folder force.
    /// </summary>
    /// <param name="remoteUser">The remote user.</param>
    public abstract void GoToUpFolderForce();

    /// <summary>
    /// Go to up folder.
    /// </summary>
    /// <param name="remotePass">The remote pass.</param>
    /// <returns>The go to up folder result.</returns>
    public abstract void GoToUpFolder();

    /// <summary>
    /// Login if is not.
    /// </summary>
    /// <param name="remotePass">The remote pass.</param>
    /// <returns>The login if is not result.</returns>
    public abstract void LoginIfIsNot(bool isInitialLogin);

    /// <summary>
    /// Get file size.
    /// </summary>
    /// <param name="remotePass">The remote pass.</param>
    public abstract long GetFileSize(string filename);

    /// <summary>
    /// Go to path.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    /// <returns>The go to path result.</returns>
    public abstract void GoToPath(string remoteFolder);

    #endregion
}
