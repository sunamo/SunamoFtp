namespace SunamoFtp.Base;

/// <summary>
/// Represents the ftp base new type.
/// </summary>
public abstract class FtpBaseNew : FtpAbstract, IDisposable
{
    /// <summary>
    /// Dispose.
    /// </summary>
    public abstract void Dispose();
    /// <summary>
    /// Debug all entries.
    /// </summary>
    public abstract void DebugAllEntries();
    /// <summary>
    /// Debug dir chmod.
    /// </summary>
    /// <param name="directoryName">The directory name.</param>
    public abstract void DebugDirChmod(string directoryName);

    /// <summary>
    /// Upload file.
    /// </summary>
    /// <param name="path">The path.</param>
    public abstract
        Task
        UploadFile(string path);
}