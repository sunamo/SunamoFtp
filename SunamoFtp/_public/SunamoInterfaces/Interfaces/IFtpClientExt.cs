namespace SunamoFtp._public.SunamoInterfaces.Interfaces;

/// <summary>
/// Represents the i ftp client ext type.
/// </summary>
public interface IFtpClientExt
{
    /// <summary>
    /// Gets the slash www slash.
    /// </summary>
    string SlashWwwSlash { get; }
    /// <summary>
    /// Gets the www slash.
    /// </summary>
    string WwwSlash { get; }
    /// <summary>
    /// Gets the www.
    /// </summary>
    string Www { get; }
    /// <summary>
    /// Is in format of album.
    /// </summary>
    /// <param name="folderName">The folder name.</param>
    /// <returns>The is in format of album result.</returns>
    bool IsInFormatOfAlbum(string folderName);
}
