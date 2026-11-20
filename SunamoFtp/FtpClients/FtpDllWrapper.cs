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

    /// <summary>
    /// Chdir lite.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    public override void ChdirLite(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Create directory if not exists.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    public override void CreateDirectoryIfNotExists(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Write debug log.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="text">The text.</param>
    /// <param name="args">The args.</param>
    public override void WriteDebugLog(string context, string text, params object[] args)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Debug actual folder.
    /// </summary>
    public override void DebugActualFolder()
    {
    }

    /// <summary>
    /// Debug all entries.
    /// </summary>
    public override void DebugAllEntries()
    {
    }

    /// <summary>
    /// Debug dir chmod.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    public override void DebugDirChmod(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Delete recursively.
    /// </summary>
    /// <param name="foldersToSkip">The folders to skip.</param>
    /// <param name="directoryName">The directory name.</param>
    /// <param name="i">The i.</param>
    /// <param name="directoriesToDelete">The directories to delete.</param>
    public override void DeleteRecursively(List<string> foldersToSkip, string directoryName, int i,
        List<DirectoriesToDeleteFtp> directoriesToDelete)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Delete remote file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The delete remote file result.</returns>
    public override bool DeleteRemoteFile(string fileName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    /// <summary>
    /// Download.
    /// </summary>
    /// <param name="remFileName">The rem file name.</param>
    /// <param name="locFileName">The loc file name.</param>
    /// <param name="deleteLocalIfExists">The delete local if exists.</param>
    /// <returns>The download result.</returns>
    public override bool Download(string remFileName, string locFileName, bool deleteLocalIfExists)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    /// <summary>
    /// Get file size.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns>The get file size result.</returns>
    public override long GetFileSize(string filename)
    {
        ThrowEx.NotImplementedMethod();
        return 0;
    }

    /// <summary>
    /// Get FS entries list recursively.
    /// </summary>
    /// <param name="foldersToSkip">The folders to skip.</param>
    /// <returns>The get FS entries list recursively result.</returns>
    public override Dictionary<string, List<string>> GetFSEntriesListRecursively(List<string> foldersToSkip)
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    /// <summary>
    /// Go to path.
    /// </summary>
    /// <param name="remoteFolder">The remote folder.</param>
    public override void GoToPath(string remoteFolder)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Go to up folder.
    /// </summary>
    public override void GoToUpFolder()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Go to up folder force.
    /// </summary>
    public override void GoToUpFolderForce()
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// List directory details.
    /// </summary>
    /// <returns>The list directory details result.</returns>
    public override List<string> ListDirectoryDetails()
    {
        ThrowEx.NotImplementedMethod();
        return null;
    }

    /// <summary>
    /// Login if is not.
    /// </summary>
    /// <param name="isInitialLogin">The is initial login.</param>
    public override void LoginIfIsNot(bool isInitialLogin)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Mkdir.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    /// <returns>The mkdir result.</returns>
    public override bool Mkdir(string directoryName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    /// <summary>
    /// Rename remote file.
    /// </summary>
    /// <param name="oldFileName">The old file name.</param>
    /// <param name="newFileName">The new file name.</param>
    public override void RenameRemoteFile(string oldFileName, string newFileName)
    {
        ThrowEx.NotImplementedMethod();
    }

    /// <summary>
    /// Rmdir.
    /// </summary>
    /// <param name="foldersToSkip">The folders to skip.</param>
    /// <param name="directoryName">The directory name.</param>
    /// <returns>The rmdir result.</returns>
    public override bool Rmdir(List<string> foldersToSkip, string directoryName)
    {
        ThrowEx.NotImplementedMethod();
        return false;
    }

    /// <summary>
    /// Uploads file to FTP server (not implemented)
    /// </summary>
    /// <param name="path">File path to upload</param>
    public override
        async Task
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