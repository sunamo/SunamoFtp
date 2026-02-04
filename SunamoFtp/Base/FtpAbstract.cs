namespace SunamoFtp.Base;

/// <summary>
/// Abstract base class for FTP client implementations
/// </summary>
public abstract class FtpAbstract
{
    /// <summary>
    /// Establishes connection to the FTP server
    /// </summary>
    public abstract void Connect();

    /// <summary>
    /// Debug output method for logging FTP operations
    /// </summary>
    /// <param name="context">Operation or context identifier</param>
    /// <param name="text">Message format string</param>
    /// <param name="args">Format arguments</param>
    public abstract void WriteDebugLog(string context, string text, params object[] args);

    /// <summary>
    /// Debug output for current folder path
    /// </summary>
    public abstract void DebugActualFolder();

    #region Variables

    /// <summary>
    /// Extended FTP client interface for application-specific operations
    /// </summary>
    public IFtpClientExt MainWindow { get; set; } = null;

    /// <summary>
    /// Path selector for managing FTP paths. Public only for Ftp class.
    /// </summary>
    public PathSelector PathSelector { get; set; } = null;

    /// <summary>
    /// Remote host address
    /// </summary>
    public string RemoteHost { get; set; }

    /// <summary>
    /// Username attempting to login (used with USER command)
    /// </summary>
    public string RemoteUser { get; set; }

    /// <summary>
    /// Password for user authentication (sent with PASS command)
    /// </summary>
    public string RemotePass { get; set; }

    /// <summary>
    /// Remote server port number
    /// </summary>
    public int RemotePort { get; set; }

    /// <summary>
    /// Indicates whether user is logged in
    /// </summary>
    public bool IsLoggedIn { get; set; }

    /// <summary>
    /// If set to false, nothing will be uploaded to hosting. Used only in this class, everything else will work normally.
    /// </summary>
    public bool ReallyUpload { get; set; } = true;

    /// <summary>
    /// Number of exceptions for single operation. Ideal for counting up to 3 and then canceling entire operation.
    /// </summary>
    protected int ExceptionCount { get; set; } = 0;

    /// <summary>
    /// Maximum allowed exception count before operation is canceled
    /// </summary>
    protected int MaxExceptionCount { get; set; } = 3;

    /// <summary>
    /// Indicates if this is startup phase
    /// </summary>
    protected bool IsStartup { get; set; } = true;

    /// <summary>
    /// Total folder size calculated recursively
    /// </summary>
    public ulong FolderSizeRecursive { get; set; } = 0;

    #endregion

    #region Set variables methods

    /// <summary>
    /// Sets remote host address
    /// </summary>
    /// <param name="remoteHost">Remote host address</param>
    public void SetRemoteHost(string remoteHost)
    {
        RemoteHost = remoteHost;
    }

    /// <summary>
    /// Gets remote host address
    /// </summary>
    /// <returns>Remote host address</returns>
    public string GetRemoteHost()
    {
        return RemoteHost;
    }

    /// <summary>
    /// Sets remote server port
    /// </summary>
    /// <param name="remotePort">Port number</param>
    public void SetRemotePort(int remotePort)
    {
        RemotePort = remotePort;
    }

    /// <summary>
    /// Gets port used for remote transfer
    /// </summary>
    /// <returns>Port number</returns>
    public int GetRemotePort()
    {
        return RemotePort;
    }

    /// <summary>
    /// Sets remote username
    /// </summary>
    /// <param name="remoteUser">Username</param>
    public void SetRemoteUser(string remoteUser)
    {
        RemoteUser = remoteUser;
    }

    /// <summary>
    /// Sets remote password
    /// </summary>
    /// <param name="remotePass">Password</param>
    public void SetRemotePass(string remotePass)
    {
        RemotePass = remotePass;
    }

    #endregion

    #region Abstract methods

    /// <summary>
    /// Creates a directory on the FTP server
    /// </summary>
    /// <param name="dirName">Directory name to create</param>
    /// <returns>True if directory was created successfully</returns>
    public abstract bool Mkdir(string dirName);

    /// <summary>
    /// Downloads a file from FTP server to local filesystem
    /// </summary>
    /// <param name="remFileName">Remote file name on FTP server</param>
    /// <param name="locFileName">Local file path to save to</param>
    /// <param name="isDeleteLocalIfExists">Whether to delete local file if it already exists</param>
    /// <returns>True if download was successful</returns>
    public abstract bool Download(string remFileName, string locFileName, bool isDeleteLocalIfExists);

    /// <summary>
    /// Deletes a file from the FTP server
    /// </summary>
    /// <param name="fileName">File name to delete</param>
    /// <returns>True if file was deleted successfully</returns>
    public abstract bool DeleteRemoteFile(string fileName);

    /// <summary>
    /// Renames a file on the FTP server
    /// </summary>
    /// <param name="oldFileName">Current file name</param>
    /// <param name="newFileName">New file name</param>
    public abstract void RenameRemoteFile(string oldFileName, string newFileName);

    /// <summary>
    /// Removes a directory from the FTP server
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during deletion</param>
    /// <param name="dirName">Directory name to remove</param>
    /// <returns>True if directory was removed successfully</returns>
    public abstract bool Rmdir(List<string> foldersToSkip, string dirName);

    /// <summary>
    /// Recursively deletes directories and their contents from FTP server
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during deletion</param>
    /// <param name="dirName">Root directory name to start deletion from</param>
    /// <param name="depth">Current recursion depth level</param>
    /// <param name="directoriesToDelete">List to collect directories marked for deletion</param>
    public abstract void DeleteRecursively(List<string> foldersToSkip, string dirName, int depth,
        List<DirectoriesToDeleteFtp> directoriesToDelete);

    /// <summary>
    /// Creates a directory on FTP server if it doesn't already exist
    /// </summary>
    /// <param name="dirName">Directory name to create</param>
    public abstract void CreateDirectoryIfNotExists(string dirName);

    /// <summary>
    /// Lists all entries (files and directories) in current FTP directory with details
    /// </summary>
    /// <returns>List of directory entry details</returns>
    public abstract List<string> ListDirectoryDetails();

    /// <summary>
    /// Recursively gets all filesystem entries (files and directories) from FTP server
    /// </summary>
    /// <param name="foldersToSkip">List of folder names to skip during traversal</param>
    /// <returns>Dictionary mapping directory paths to lists of entry names</returns>
    public abstract Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip);

    /// <summary>
    /// Changes current directory on FTP server (lightweight version)
    /// </summary>
    /// <param name="dirName">Directory name to change to</param>
    public abstract void ChdirLite(string dirName);

    /// <summary>
    /// Navigates to parent folder on FTP server (forced, no validation)
    /// </summary>
    public abstract void GoToUpFolderForce();

    /// <summary>
    /// Navigates to parent folder on FTP server with validation
    /// </summary>
    public abstract void GoToUpFolder();

    /// <summary>
    /// Performs login to FTP server if not already logged in
    /// </summary>
    /// <param name="isStartup">Indicates if this is initial startup login</param>
    public abstract void LoginIfIsNot(bool isStartup);

    /// <summary>
    /// Gets the size of a file on the FTP server
    /// </summary>
    /// <param name="filename">File name to get size for</param>
    /// <returns>File size in bytes</returns>
    public abstract long GetFileSize(string filename);

    /// <summary>
    /// Navigates to specified path on FTP server
    /// </summary>
    /// <param name="remoteFolder">Remote folder path to navigate to</param>
    public abstract void GoToPath(string remoteFolder);

    #endregion
}