namespace SunamoFtp.Base;

/// <summary>
/// Helper methods for FTP operations
/// </summary>
public static class FtpHelper
{
    /// <summary>
    /// Checks if folder name is current directory (.) or parent directory (..)
    /// </summary>
    /// <param name="folderName">Folder name to check</param>
    /// <returns>True if folder is . or ..</returns>
    public static bool IsThisOrUp(string folderName)
    {
        return folderName == "." || folderName == "..";
    }

    /// <summary>
    /// Checks if file with given name and length exists on FTP hosting
    /// </summary>
    /// <param name="localFilePath">Local file path</param>
    /// <param name="ftpEntries">List of FTP directory entries</param>
    /// <param name="fileLength">File length to compare</param>
    /// <returns>True if file with same name and length exists on hosting</returns>
    public static bool IsFileOnHosting(string localFilePath, List<string> ftpEntries, long fileLength)
    {
        localFilePath = Path.GetFileName(localFilePath);
        foreach (var item in ftpEntries)
        {
            long entryFileLength = 0;
            string entryFileName = null;
            if (IsFile(item, out entryFileName, out entryFileLength) == FileSystemType.File)
                if (entryFileName == localFilePath)
                    if (entryFileLength == fileLength)
                        return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if FTP entry is file, folder, or link
    /// </summary>
    /// <param name="entry">FTP directory entry</param>
    /// <returns>File system type</returns>
    public static FileSystemType IsFile(string entry)
    {
        string fileName = null;
        var tokens = entry.Split(' ').ToList();
        var fileSystemType = IsFileShared(entry, tokens, out fileName);
        return fileSystemType;
    }

    /// <summary>
    /// Determines if FTP entry is file, folder, or link and extracts file name
    /// </summary>
    /// <param name="entry">FTP directory entry</param>
    /// <param name="fileName">Extracted file/folder name</param>
    /// <returns>File system type</returns>
    public static FileSystemType IsFile(string entry, out string fileName)
    {
        var tokens = entry.Split(' ').ToList();
        var fileSystemType = IsFileShared(entry, tokens, out fileName);
        return fileSystemType;
    }

    /// <summary>
    /// Determines if FTP entry is file, folder, or link and extracts file name and length
    /// </summary>
    /// <param name="entry">FTP directory entry (format: drw-rw-rw-   1 user     group           0 Nov 21 18:03 App_Data)</param>
    /// <param name="fileName">Extracted file/folder name</param>
    /// <param name="length">File length in bytes</param>
    /// <returns>File system type</returns>
    public static FileSystemType IsFile(string entry, out string fileName, out long length)
    {
        var tokens = entry.Split(' ').ToList();
        var fileSystemType = IsFileShared(entry, tokens, out fileName);
        length = long.Parse(tokens[4]);

        return fileSystemType;
    }

    /// <summary>
    /// Shared logic for determining file system entry type
    /// </summary>
    /// <param name="entry">FTP directory entry</param>
    /// <param name="tokens">Entry split into tokens</param>
    /// <param name="fileName">Extracted file/folder name</param>
    /// <returns>File system type</returns>
    private static FileSystemType IsFileShared(string entry, List<string> tokens, out string fileName)
    {
        fileName = SHJoin.JoinFromIndex(8, ' ', tokens);
        var fileSystemType = FileSystemType.File;
        var firstChar = entry[0];
        if (firstChar == '-')
        {
            // It's a file
        }
        else if (firstChar == 'd')
        {
            if (IsThisOrUp(fileName))
                fileSystemType = FileSystemType.Link;
            else
                fileSystemType = FileSystemType.Folder;
        }
        else
        {
            throw new Exception("Unknown entry type (change msdos directory listing to unix)");
        }

        return fileSystemType;
    }

    /// <summary>
    /// Checks if string starts with FTP schema (ftp://)
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if path starts with ftp://</returns>
    public static bool IsSchemaFtp(string path)
    {
        return path.StartsWith("ftp" + ":" + "//");
    }

    /// <summary>
    /// Extracts directory names from FTP entries list
    /// </summary>
    /// <param name="ftpEntries">List of FTP directory entries</param>
    /// <returns>List of directory names</returns>
    public static IList<string> GetDirectories(List<string> ftpEntries)
    {
        var result = new List<string>();
        foreach (var item in ftpEntries)
        {
            string fileName = null;
            if (IsFile(item, out fileName) == FileSystemType.Folder) result.Add(fileName);
        }

        return result;
    }

    /// <summary>
    /// Removes FTP schema prefix from hostname
    /// </summary>
    /// <param name="remoteHost">Remote host with or without ftp:// prefix</param>
    /// <returns>Host without ftp:// prefix</returns>
    public static string ReplaceSchemaFtp(string remoteHost)
    {
        return remoteHost.Replace("ftp" + ":" + "//", "");
    }
}