namespace SunamoFtp.Base;

public static class FtpHelper
{
    public static bool IsThisOrUp(string folderName) => folderName == "." || folderName == "..";

    public static bool IsFileOnHosting(string localFilePath, List<string> ftpEntries, long fileLength)
    {
        localFilePath = Path.GetFileName(localFilePath);
        foreach (var item in ftpEntries)
        {
            if (IsFile(item, out var entryFileName, out var entryFileLength) == FileSystemType.File)
                if (entryFileName == localFilePath)
                    if (entryFileLength == fileLength)
                        return true;
        }

        return false;
    }

    public static FileSystemType IsFile(string entry)
    {
        var tokens = entry.Split(' ').ToList();
        return IsFileShared(entry, tokens, out _);
    }

    public static FileSystemType IsFile(string entry, out string fileName)
    {
        var tokens = entry.Split(' ').ToList();
        return IsFileShared(entry, tokens, out fileName);
    }

    // entry format: drw-rw-rw-   1 user     group           0 Nov 21 18:03 App_Data
    public static FileSystemType IsFile(string entry, out string fileName, out long length)
    {
        var tokens = entry.Split(' ').ToList();
        var fileSystemType = IsFileShared(entry, tokens, out fileName);
        length = long.Parse(tokens[4]);

        return fileSystemType;
    }

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

    public static bool IsSchemaFtp(string path) => path.StartsWith("ftp" + ":" + "//");

    public static IList<string> GetDirectories(List<string> ftpEntries)
    {
        var result = new List<string>();
        foreach (var item in ftpEntries)
        {
            if (IsFile(item, out var fileName) == FileSystemType.Folder) result.Add(fileName);
        }

        return result;
    }

    public static string ReplaceSchemaFtp(string remoteHost) => remoteHost.Replace("ftp" + ":" + "//", "");
}
