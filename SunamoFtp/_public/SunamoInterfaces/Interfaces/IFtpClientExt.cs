namespace SunamoFtp._public.SunamoInterfaces.Interfaces;

/// <summary>
/// Extended FTP client interface for application-specific FTP operations
/// </summary>
public interface IFtpClientExt
{
    /// <summary>
    /// Gets the path with leading and trailing slash for www directory (e.g., "/www/")
    /// </summary>
    string SlashWwwSlash { get; }

    /// <summary>
    /// Gets the path with trailing slash for www directory (e.g., "www/")
    /// </summary>
    string WwwSlash { get; }

    /// <summary>
    /// Gets the www directory name without slashes
    /// </summary>
    string Www { get; }

    /// <summary>
    /// Determines whether specified folder name matches album format
    /// </summary>
    /// <param name="folderName">Folder name to check</param>
    /// <returns>True if folder name is in album format</returns>
    bool IsInFormatOfAlbum(string folderName);
}
