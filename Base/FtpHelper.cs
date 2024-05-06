
namespace SunamoFtp.Base;

using SunamoFtp._sunamo;
using SunamoValues;


public static class FtpHelper
{
    /// <summary>
    /// Vrátí zda A1 je .. nebo .
    /// </summary>
    /// <param name="folderName2"></param>
    public static bool IsThisOrUp(string folderName2)
    {
        return folderName2 == AllStrings.dot || folderName2 == AllStrings.dd;
    }

    /// <summary>
    /// OK
    /// </summary>
    /// <param name="item2"></param>
    /// <param name="fse"></param>
    /// <param name="fileLenght"></param>
    public static bool IsFileOnHosting(string item2, List<string> fse, long fileLenght)
    {
        item2 = Path.GetFileName(item2);
        foreach (string item in fse)
        {
            long fl = 0;
            string fn = null;
            if (IsFile(item, out fn, out fl) == FileSystemType.File)
            {
                if (fn == item2)
                {
                    if (fl == fileLenght)
                    {
                        return true;
                    }
                }

            }
        }
        return false;
    }

    public static FileSystemType IsFile(string entry)
    {
        string fileName = null;
        var tokeny = entry.Split(AllChars.space).ToList(); //SHSplit.Split(entry, AllStrings.space);
        FileSystemType isFile = IsFileShared(entry, tokeny, out fileName);
        return isFile;
    }

    public static FileSystemType IsFile(string entry, out string fileName)
    {
        var tokeny = entry.Split(AllChars.space).ToList(); //SHSplit.Split(entry, AllStrings.space);
        FileSystemType isFile = IsFileShared(entry, tokeny, out fileName);
        return isFile;
    }

    public static FileSystemType IsFile(string entry, out string fileName, out long length)
    {
        //drw-rw-rw-   1 user     group           0 Nov 21 18:03 App_Data
        var tokeny = entry.Split(AllChars.space).ToList(); //SHSplit.Split(entry, AllStrings.space);
        FileSystemType isFile = IsFileShared(entry, tokeny, out fileName);
        length = long.Parse(tokeny[4]);

        return isFile;
    }

    private static FileSystemType IsFileShared(string entry, List<string> tokeny, out string fileName)
    {
        fileName = SHJoin.JoinFromIndex(8, AllCharsSE.space, tokeny);
        FileSystemType isFile = FileSystemType.File;
        char f = entry[0];
        if (f == AllCharsSE.dash)
        {

        }
        else if (f == 'd')
        {
            if (IsThisOrUp(fileName))
            {
                isFile = FileSystemType.Link;
            }
            else
            {
                isFile = FileSystemType.Folder;
            }

        }
        else
        {
            throw new Exception("Nový druh entry (change msdos directory listing to unix)");
        }
        return isFile;
    }

    static Type type = typeof(FtpHelper);

    public static bool IsSchemaFtp(string remFileName)
    {
        return remFileName.StartsWith("ftp" + ":" + "//");
    }

    public static IList<string> GetDirectories(List<string> fse)
    {
        List<string> vr = new List<string>();
        foreach (var item in fse)
        {
            string fn = null;
            if (IsFile(item, out fn) == FileSystemType.Folder)
            {
                vr.Add(fn);
            }
        }
        return vr;
    }

    public static string ReplaceSchemaFtp(string remoteHost2)
    {
        return remoteHost2.Replace("ftp" + ":" + "//", "");
    }
}
